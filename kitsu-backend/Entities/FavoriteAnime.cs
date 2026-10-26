namespace kitsu_backend.Entities
{
    /* Join table for Anime and ApplicationUser to create a favorite anime entity */
    public class FavoriteAnime
    {
        public string ApplicationUserId { get; set; } = string.Empty;
        public int AnimeId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public Anime Anime { get; set; } = null!;
        public DateTime DateAdded { get; set; }
    }
}
