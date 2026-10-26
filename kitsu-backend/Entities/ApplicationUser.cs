using Microsoft.AspNetCore.Identity;

namespace kitsu_backend.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string DisplayId { get; set; } //Different from ID. URL-friendly
        public List<Anime> Animes { get; } = [];
        public List<LibraryEntry> AnimeLibraryEntries { get; } = [];
        public string? Location { get; set; }
        public string? Bio { get; set; }
        public DateOnly? Birthday { get; set; }
        public string? Gender { get; set; }
        public List<Anime> FavoriteAnimes { get; } = [];
        public List<FavoriteAnime> FavoriteAnimeEntries { get; } = [];
        public string? ProfilePhotoId { get; set; }
        public string? CoverPhotoId { get; set; }
        public ICollection<MediaReaction> MediaReactions { get; } = [];
    }
}
