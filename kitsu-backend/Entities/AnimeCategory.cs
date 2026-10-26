using System.ComponentModel.DataAnnotations.Schema;

namespace kitsu_backend.Entities
{
    /* Join table for Anime and Category */
    public class AnimeCategory
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public int CategoryId { get; set; }
        public int AnimeId { get; set; }
        public Category Category { get; set; } = null!;
        public Anime Anime { get; set; } = null!;
    }
}
