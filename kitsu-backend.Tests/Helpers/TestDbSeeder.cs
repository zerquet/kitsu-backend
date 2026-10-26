using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Identity;

namespace kitsu_backend.Tests.Helpers
{
    public static class TestDbSeeder
    {
        public static void SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            foreach (var roleName in new[] { "Admin", "User" })
                roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
        }

        public static void SeedAnime(ApplicationDbContext context)
        {
            var animeList = new List<Anime>
            {
                new Anime { Id = 1, Title = "Naruto", Categories = [], EpisodeCount = 220 },
                new Anime { Id = 2, Title = "One Piece", Categories = [], EpisodeCount = 1000 },
                new Anime { Id = 3, Title = "Attack on Titan", Categories = [], EpisodeCount = 85 }
            };
            context.Animes.AddRange(animeList);
            context.SaveChanges();
        }
    }
}
