using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kitsu_backend.Entities
{
    /* Join table for Anime and ApplicationUser to create a library entry entity */
    public class LibraryEntry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        [MaxLength(50)]
        public string ApplicationUserDisplayId { get; set; } = string.Empty;
        public string ApplicationUserId { get; set; } = string.Empty;
        public int AnimeId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public Anime Anime { get; set; } = null!;
        public string? WatchStatus { get; set; }
        public int? UserRating { get; set; } //null = no rating. 1-4 = awful, bad, good, great. 0 not stored.
        public int EpisodesWatched { get; set; }
        public MediaReaction? MediaReaction { get; set; }
    }
}
