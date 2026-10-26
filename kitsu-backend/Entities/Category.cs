using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace kitsu_backend.Entities
{
    public class Category
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public string? Name { get; set; } // "Slice of Life" (display)
        public string? Description { get; set; }
        [JsonIgnore]
        public List<Anime> Animes { get; } = [];
        [JsonIgnore]
        public List<AnimeCategory> AnimeCategories { get; } = [];
        public string? Slug { get; set; } // "slice-of-life" (identifier)
    }
}
