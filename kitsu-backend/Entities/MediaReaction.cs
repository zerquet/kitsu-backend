using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kitsu_backend.Entities
{
    public class MediaReaction
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public string ApplicationUserId { get; set; } = string.Empty;
        public int AnimeId { get; set; }
        public int LibraryEntryId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;
        public Anime Anime { get; set; } = null!;
        public LibraryEntry LibraryEntry { get; set; } = null!;
        public int UpvoteCount { get; set; }
        ///Should I add anime properties here like Name, Poster Id, Year, and Type? 
        
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        [MaxLength(140)]
        public string Reaction { get; set; } = string.Empty;
    }
}
