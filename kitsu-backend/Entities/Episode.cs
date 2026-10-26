using System.ComponentModel.DataAnnotations.Schema;

namespace kitsu_backend.Entities
{
    public class Episode
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public int AnimeId { get; set; }
        public Anime Anime { get; set; } = null!;
        public int? Number { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateOnly? AirDate { get; set; }
        public string? JapaneseTitle { get; set; }
    }
}
