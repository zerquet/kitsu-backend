using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.MediaReactions;

public record GetMediaReactionsByAnimeRequest(int AnimeId);

public class MediaReactionByAnimeResponse
{
    public int MediaReactionId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string Reaction { get; set; } = string.Empty;
    public int UpvoteCount { get; set; }
}

public class GetMediaReactionsByAnimeHandler
{
    private readonly ApplicationDbContext _context;

    public GetMediaReactionsByAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<MediaReactionByAnimeResponse>> Handle(GetMediaReactionsByAnimeRequest request, CancellationToken cancellationToken)
    {
        var mediaReactions = await _context.MediaReactions
            .Where(m => m.AnimeId == request.AnimeId)
            .OrderByDescending(m => m.UpvoteCount)
            .Take(10)
            .Select(m => new MediaReactionByAnimeResponse
            {
                MediaReactionId = m.Id,
                UserId = m.ApplicationUser.DisplayId,
                Reaction = m.Reaction,
                UpvoteCount = m.UpvoteCount,
                Author = m.ApplicationUser.UserName
            })
            .ToListAsync(cancellationToken);

        return mediaReactions;
    }

}

public static class GetMediaReactionsByAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes/{animeId}/media-reactions", async (
            [FromRoute] int animeId,
            GetMediaReactionsByAnimeHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetMediaReactionsByAnimeRequest(animeId);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}