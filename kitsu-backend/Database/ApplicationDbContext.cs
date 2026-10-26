using kitsu_backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            List<IdentityRole> roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = "6dc742e3-e459-4149-8849-0e2db1ec4d64",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "96abbc38-8ebf-49df-b9ff-bcbfd05fc961",
                    Name = "User",
                    NormalizedName = "USER"
                }
            };

            modelBuilder.Entity<IdentityRole>().HasData(roles);

            //Define relationships...
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(ku => ku.Animes)
                .WithMany(a => a.ApplicationUsers)
                .UsingEntity<LibraryEntry>();

            modelBuilder.Entity<Anime>()
                .HasMany(a => a.Categories)
                .WithMany(c => c.Animes)
                .UsingEntity<AnimeCategory>()
                .ToTable("AnimeCategory"); //Telling EF Core that "AnimeCategories" here is "AnimeCategory" in the db.

            modelBuilder.Entity<Anime>()
                .HasOne(a => a.Franchise)
                .WithMany(f => f.Animes);

            modelBuilder.Entity<Anime>()
                .HasMany(a => a.EpisodeList)
                .WithOne(e => e.Anime);

            modelBuilder.Entity<Episode>().ToTable("Episode");
            modelBuilder.Entity<Franchise>().ToTable("Franchise");

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(ku => ku.FavoriteAnimes)
                .WithMany(a => a.FavoriteUsers)
                .UsingEntity<FavoriteAnime>()
                .ToTable("FavoriteAnime");

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.MediaReactions)
                .WithOne(mr => mr.ApplicationUser);

            modelBuilder.Entity<Anime>()
                .HasMany(a => a.MediaReactions)
                .WithOne(mr => mr.Anime);

            modelBuilder.Entity<LibraryEntry>()
                .HasOne(le => le.MediaReaction)
                .WithOne(mr => mr.LibraryEntry)
                .HasForeignKey<MediaReaction>(mr => mr.LibraryEntryId)
                .OnDelete(DeleteBehavior.Restrict); //delete media reaction if library entry is deleted

            modelBuilder.Entity<Anime>()
                .HasIndex(a => a.KitsuId)
                .IsUnique()
                .HasFilter("[KitsuId] IS NOT NULL"); //Ensure KitsuId is unique to prevent duplicate entries for the same anime.

            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Slug).IsUnique();

        }

        public DbSet<Anime> Animes => Set<Anime>();
        public DbSet<LibraryEntry> LibraryEntries => Set<LibraryEntry>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<AnimeCategory> AnimeCategories => Set<AnimeCategory>();
        public DbSet<Franchise> Franchises => Set<Franchise>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<FavoriteAnime> FavoriteAnimes => Set<FavoriteAnime>();
        public DbSet<MediaReaction> MediaReactions => Set<MediaReaction>();
    }
}
