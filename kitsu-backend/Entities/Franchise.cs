using System.ComponentModel.DataAnnotations.Schema;

namespace kitsu_backend.Entities
{
    public class Franchise
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public virtual int Id { get; set; }
        public string? Name { get; set; }
        public ICollection<Anime> Animes { get; } = [];
    }
}
