using kitsu_backend.Database;
using kitsu_backend.Entities;
using kitsu_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Admin;

/// <summary>
/// Admin only. Links anime to categories using Kitsu's own tags, matching anime by KitsuId and categories by slug.
/// Only adds missing links, so it is safe to re-run: after adding a new category, run it again to fill that category in.
/// </summary>
public record SyncCategoriesFromKitsuRequest(bool DryRun);

public record SyncCategoriesFromKitsuResponse(
    bool DryRun,
    int AnimeChecked,
    int AnimeUpdated,
    int LinksAdded,
    List<int> KitsuIdsNotFound,
    List<string> CategorySlugsNotOnKitsu);

public class SyncCategoriesFromKitsuHandler
{
    private readonly ApplicationDbContext _context;
    private readonly IKitsuApiClient _kitsu;

    public SyncCategoriesFromKitsuHandler(ApplicationDbContext context, IKitsuApiClient kitsu)
    {
        _context = context;
        _kitsu = kitsu;
    }

    public async Task<SyncCategoriesFromKitsuResponse> Handle(SyncCategoriesFromKitsuRequest request, CancellationToken cancellationToken)
    {
        var localCategoryIdsBySlug = await _context.Categories
            .Where(c => c.Slug != null)
            .Select(c => new { Slug = c.Slug!, c.Id })
            .ToDictionaryAsync(c => c.Slug, c => c.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var kitsuCategories = await _kitsu.GetCategoriesAsync(cancellationToken);
        var localIdsByKitsuCategory = MapKitsuToLocalCategories(kitsuCategories, localCategoryIdsBySlug);

        var kitsuSlugs = kitsuCategories.Select(c => c.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var slugsNotOnKitsu = localCategoryIdsBySlug.Keys.Where(slug => !kitsuSlugs.Contains(slug)).Order().ToList();

        var animeIdsByKitsuId = await _context.Animes
            .Where(a => a.KitsuId != null)
            .Select(a => new { KitsuId = a.KitsuId!.Value, a.Id })
            .ToDictionaryAsync(a => a.KitsuId, a => a.Id, cancellationToken);

        // The join table has no unique constraint, so check existing links ourselves to avoid duplicates
        var existingLinks = (await _context.AnimeCategories
            .Select(ac => new { ac.AnimeId, ac.CategoryId })
            .ToListAsync(cancellationToken))
            .Select(ac => (ac.AnimeId, ac.CategoryId))
            .ToHashSet();

        var kitsuIdsNotFound = new List<int>();
        var updatedAnimeIds = new HashSet<int>();
        var linksAdded = 0;

        foreach (var batch in animeIdsByKitsuId.Keys.Order().Chunk(KitsuApiClient.MaxAnimePerRequest))
        {
            var kitsuCategoryIdsByAnime = await _kitsu.GetAnimeCategoryIdsAsync(batch, cancellationToken);

            foreach (var kitsuId in batch)
            {
                if (!kitsuCategoryIdsByAnime.TryGetValue(kitsuId, out var kitsuCategoryIds))
                {
                    kitsuIdsNotFound.Add(kitsuId);
                    continue;
                }

                var animeId = animeIdsByKitsuId[kitsuId];
                var categoryIds = kitsuCategoryIds
                    .Where(localIdsByKitsuCategory.ContainsKey)
                    .SelectMany(id => localIdsByKitsuCategory[id]);

                foreach (var categoryId in categoryIds)
                {
                    if (!existingLinks.Add((animeId, categoryId)))
                        continue;

                    if (!request.DryRun)
                        _context.AnimeCategories.Add(new AnimeCategory { AnimeId = animeId, CategoryId = categoryId });
                    updatedAnimeIds.Add(animeId);
                    linksAdded++;
                }
            }

            // Save per batch so a failure partway through keeps what was done; a re-run picks up the rest
            if (!request.DryRun)
                await _context.SaveChangesAsync(cancellationToken);
        }

        return new SyncCategoriesFromKitsuResponse(
            request.DryRun,
            animeIdsByKitsuId.Count,
            updatedAnimeIds.Count,
            linksAdded,
            kitsuIdsNotFound,
            slugsNotOnKitsu);
    }

    /// <summary>
    /// Maps each Kitsu category id to the local categories it implies: itself and each of its ancestors, where they exist locally.
    /// Kitsu's tree is "is-a" (samurai > swordplay > action), so an anime tagged "samurai" also gets "action".
    /// Kitsu categories with no local match anywhere in their chain are left out.
    /// </summary>
    internal static Dictionary<int, List<int>> MapKitsuToLocalCategories(
        IEnumerable<KitsuCategory> kitsuCategories,
        IReadOnlyDictionary<string, int> localCategoryIdsBySlug)
    {
        var kitsuCategoriesById = kitsuCategories.ToDictionary(c => c.Id);
        var localIdsByKitsuCategory = new Dictionary<int, List<int>>();

        foreach (var category in kitsuCategoriesById.Values)
        {
            var localIds = new List<int>();
            var visited = new HashSet<int>(); // guards against a cycle in Kitsu's data

            for (var current = category; current != null && visited.Add(current.Id);
                 current = current.ParentId is int parentId ? kitsuCategoriesById.GetValueOrDefault(parentId) : null)
            {
                if (localCategoryIdsBySlug.TryGetValue(current.Slug, out var localId))
                    localIds.Add(localId);
            }

            if (localIds.Count > 0)
                localIdsByKitsuCategory[category.Id] = localIds;
        }

        return localIdsByKitsuCategory;
    }
}

public static class SyncCategoriesFromKitsuEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/categories/sync-from-kitsu", [Authorize(Roles = "Admin")] async (
            bool? dryRun,
            SyncCategoriesFromKitsuHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new SyncCategoriesFromKitsuRequest(dryRun ?? false), ct);

            return Results.Ok(result);
        });
    }
}
