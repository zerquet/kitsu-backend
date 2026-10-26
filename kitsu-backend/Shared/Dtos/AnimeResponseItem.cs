namespace kitsu.Shared.Dtos;

public class AnimeResponseItem
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? Score { get; set; }
    public string? ImageUrl { get; set; }
    public int? Year { get; set; }
}
