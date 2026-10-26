using System.ComponentModel.DataAnnotations.Schema;

namespace kitsu_backend.Entities
{
    public class Anime
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public int? KitsuId { get; set; } // stable identifier for importing entries from Kitsu.io's API. 
        public string Title { get; set; } = "No title";
        public string? Description { get; set; }
        public int? Score { get; set; }
        public List<Category> Categories { get; set; } = [];
        public List<AnimeCategory> AnimeCategories { get; } = [];
        public string? ReleaseStatus { get; set; }
        public string? ImageUrl { get; set; }
        public List<ApplicationUser> ApplicationUsers { get; } = [];
        public List<LibraryEntry> AnimeLibraryEntries { get; } = [];
        public int? Year { get; set; }
        public string? CoverImageId { get; set; }
        public int? EpisodeCount { get; set; } //default should be 0. make required.
        public string? MediaType { get; set; }
        public List<string> Studios { get; set; } = [];
        public string? EnglishTitle { get; set; }
        public string? JapaneseTitle { get; set; }
        public string? JapaneseTitleRomaji { get; set; }
        public string? Season { get; set; }
        public DateOnly? StartAirDate { get; set; }
        public DateOnly? EndAirDate { get; set; }
        public string? TvRating { get; set; }
        public int? EpisodeLength { get; set; }
        public int? FranchiseId { get; set; }
        public Franchise? Franchise { get; set; }
        public ICollection<Episode> EpisodeList { get; set; } = [];
        public List<ApplicationUser> FavoriteUsers { get; } = [];
        public List<FavoriteAnime> FavoriteAnimeEntries { get; } = [];
        public ICollection<MediaReaction> MediaReactions { get; } = [];
    }
}
