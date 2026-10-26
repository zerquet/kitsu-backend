namespace kitsu.Shared.Dtos;

public class ProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? Gender { get; set; }
    public string? ProfilePhotoId { get; set; }
    public string? CoverPhotoId { get; set; }
}
