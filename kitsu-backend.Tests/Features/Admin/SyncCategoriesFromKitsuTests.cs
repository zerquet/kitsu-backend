using kitsu_backend.Database;
using kitsu_backend.Entities;
using kitsu_backend.Features.Admin;
using kitsu_backend.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Tests.Features.Admin
{
    public class SyncCategoriesFromKitsuTests : IDisposable
    {
        // A slice of Kitsu's real tree: elements > action > swordplay > samurai, elements > science-fiction > mecha
        private static readonly List<KitsuCategory> KitsuTree =
        [
            new(1, "elements", null),
            new(2, "action", 1),
            new(3, "swordplay", 2),
            new(4, "samurai", 3),
            new(5, "science-fiction", 1),
            new(6, "mecha", 5),
            new(7, "comedy", 1),
        ];

        private readonly SqliteConnection _connection;
        private readonly ApplicationDbContext _context;

        public SyncCategoriesFromKitsuTests()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();
            _context = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public void MapKitsuToLocalCategories_ShouldIncludeSelfAndLocalAncestors()
        {
            // Arrange: only some of Kitsu's categories exist locally
            var local = new Dictionary<string, int> { ["action"] = 10, ["science-fiction"] = 20, ["mecha"] = 30 };

            // Act
            var result = SyncCategoriesFromKitsuHandler.MapKitsuToLocalCategories(KitsuTree, local);

            // Assert
            Assert.Equal(new[] { 10 }, result[4]); // samurai rolls up to action, skipping swordplay which isn't local
            Assert.Equal(new[] { 30, 20 }, result[6]); // mecha keeps itself and science-fiction
            Assert.False(result.ContainsKey(1)); // nothing local in elements' chain
            Assert.False(result.ContainsKey(7)); // comedy isn't local
        }

        [Fact]
        public void MapKitsuToLocalCategories_ShouldStopOnCycle()
        {
            var cyclic = new List<KitsuCategory> { new(1, "a", 2), new(2, "b", 1) };

            var result = SyncCategoriesFromKitsuHandler.MapKitsuToLocalCategories(cyclic, new Dictionary<string, int> { ["a"] = 10 });

            Assert.Equal(new[] { 10 }, result[1]);
            Assert.Equal(new[] { 10 }, result[2]);
        }

        [Fact]
        public async Task Handle_ShouldAddMissingLinksOnly()
        {
            // Arrange
            var (samuraiAnime, mechaAnime, _, unlinkedAnime) = Seed();
            var kitsu = new FakeKitsuApiClient(KitsuTree, new()
            {
                [100] = [4], // samurai
                [200] = [6, 5], // mecha, science-fiction (already linked locally)
                // 300 isn't on Kitsu
            });
            var handler = new SyncCategoriesFromKitsuHandler(_context, kitsu);

            // Act
            var result = await handler.Handle(new SyncCategoriesFromKitsuRequest(DryRun: false), CancellationToken.None);

            // Assert
            Assert.Equal(3, result.AnimeChecked); // anime without a KitsuId are skipped
            Assert.Equal(2, result.AnimeUpdated);
            Assert.Equal(2, result.LinksAdded);
            Assert.Equal(new[] { 300 }, result.KitsuIdsNotFound);
            Assert.Equal(new[] { "made-up" }, result.CategorySlugsNotOnKitsu);

            Assert.Equal(new[] { "action" }, CategorySlugsOf(samuraiAnime.Id));
            Assert.Equal(new[] { "mecha", "science-fiction" }, CategorySlugsOf(mechaAnime.Id));
            Assert.Empty(CategorySlugsOf(unlinkedAnime.Id));

            // Running again adds nothing
            var rerun = await handler.Handle(new SyncCategoriesFromKitsuRequest(DryRun: false), CancellationToken.None);
            Assert.Equal(0, rerun.LinksAdded);
            Assert.Equal(3, _context.AnimeCategories.Count());
        }

        [Fact]
        public async Task Handle_DryRun_ShouldReportWithoutSaving()
        {
            Seed();
            var kitsu = new FakeKitsuApiClient(KitsuTree, new() { [100] = [4], [200] = [6] });
            var handler = new SyncCategoriesFromKitsuHandler(_context, kitsu);

            var result = await handler.Handle(new SyncCategoriesFromKitsuRequest(DryRun: true), CancellationToken.None);

            Assert.Equal(2, result.LinksAdded);
            Assert.Equal(1, _context.AnimeCategories.Count()); // only the seeded link
        }

        [Fact]
        public async Task Handle_ShouldRequestAnimeInBatchesKitsuAllows()
        {
            _context.Animes.AddRange(Enumerable.Range(1, 45).Select(i => new Anime { Title = $"Anime {i}", KitsuId = i }));
            await _context.SaveChangesAsync();
            var kitsu = new FakeKitsuApiClient(KitsuTree, Enumerable.Range(1, 45).ToDictionary(i => i, _ => new List<int>()));

            await new SyncCategoriesFromKitsuHandler(_context, kitsu)
                .Handle(new SyncCategoriesFromKitsuRequest(DryRun: false), CancellationToken.None);

            Assert.Equal(new[] { 20, 20, 5 }, kitsu.RequestedBatches.Select(b => b.Count));
        }

        private (Anime samurai, Anime mecha, Anime notOnKitsu, Anime noKitsuId) Seed()
        {
            var categories = new[] { "action", "science-fiction", "mecha", "made-up" }
                .Select(slug => new Category { Name = slug, Slug = slug })
                .ToList();
            var samurai = new Anime { Title = "Samurai Champloo", KitsuId = 100 };
            var mecha = new Anime { Title = "Gundam", KitsuId = 200 };
            var notOnKitsu = new Anime { Title = "Removed from Kitsu", KitsuId = 300 };
            var noKitsuId = new Anime { Title = "Added by hand" };

            _context.Categories.AddRange(categories);
            _context.Animes.AddRange(samurai, mecha, notOnKitsu, noKitsuId);
            _context.SaveChanges();

            _context.AnimeCategories.Add(new AnimeCategory { AnimeId = mecha.Id, CategoryId = categories[1].Id });
            _context.SaveChanges();
            _context.ChangeTracker.Clear();

            return (samurai, mecha, notOnKitsu, noKitsuId);
        }

        private List<string?> CategorySlugsOf(int animeId) => _context.AnimeCategories
            .Where(ac => ac.AnimeId == animeId)
            .Select(ac => ac.Category.Slug)
            .OrderBy(slug => slug)
            .ToList();

        private sealed class FakeKitsuApiClient : IKitsuApiClient
        {
            private readonly List<KitsuCategory> _categories;
            private readonly Dictionary<int, List<int>> _categoryIdsByAnime;
            public List<IReadOnlyCollection<int>> RequestedBatches { get; } = [];

            public FakeKitsuApiClient(List<KitsuCategory> categories, Dictionary<int, List<int>> categoryIdsByAnime)
            {
                _categories = categories;
                _categoryIdsByAnime = categoryIdsByAnime;
            }

            public Task<List<KitsuCategory>> GetCategoriesAsync(CancellationToken cancellationToken) => Task.FromResult(_categories);

            public Task<Dictionary<int, List<int>>> GetAnimeCategoryIdsAsync(IReadOnlyCollection<int> kitsuAnimeIds, CancellationToken cancellationToken)
            {
                RequestedBatches.Add(kitsuAnimeIds);
                // Like Kitsu, anime it doesn't know are left out of the result
                return Task.FromResult(kitsuAnimeIds
                    .Where(_categoryIdsByAnime.ContainsKey)
                    .ToDictionary(id => id, id => _categoryIdsByAnime[id]));
            }
        }
    }
}
