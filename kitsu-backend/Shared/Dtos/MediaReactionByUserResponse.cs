using kitsu_backend.Entities;

namespace kitsu.Shared.Dtos;

public class MediaReactionByUserResponse
{
    public int MediaReactionId { get; set; } 
    public string UserId { get; set; } = string.Empty;
    public int LibraryEntryId { get; set;}
    public int AnimeId { get; set;}
    public string Reaction { get; set; } = string.Empty;
    public int UpvoteCount { get; set;}
    public Anime Anime { get; set; } = null!;
}

