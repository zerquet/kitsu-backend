using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to get the user's anime library.
/// </summary>
/// <param name="UserId"></param>
public record GetLibraryRequest(string UserId);
public class GetLibraryResponse
{
    public int LibraryEntryId { get; set; }
    public int AnimeId { get; set; }
    public string? WatchStatus { get; set; }
    public int? UserRating { get; set; }
    public int EpisodesWatched { get; set; }
    public Anime Anime { get; set; } = null!;
    public MediaReactionByUserResponse? MediaReaction { get; set; }

}
public class GetLibraryHandler
{
    private readonly ApplicationDbContext _context;
    public GetLibraryHandler(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<GetLibraryResponse>> Handle(GetLibraryRequest request, CancellationToken cancellationToken)
    {
        var library = await (from l in _context.LibraryEntries
            where l.ApplicationUserDisplayId == request.UserId 
            select new GetLibraryResponse
            {
                LibraryEntryId = l.Id,
                AnimeId = l.AnimeId,
                Anime = l.Anime, //is full object necessary?
                WatchStatus = l.WatchStatus,
                UserRating = l.UserRating,
                EpisodesWatched = l.EpisodesWatched,
                MediaReaction = l.MediaReaction == null ? null : new MediaReactionByUserResponse {
                    MediaReactionId = l.MediaReaction.Id,
                    UserId = l.MediaReaction.ApplicationUserId,
                    LibraryEntryId = l.Id,
                    AnimeId = l.MediaReaction.AnimeId,
                    Reaction = l.MediaReaction.Reaction,
                    UpvoteCount = l.MediaReaction.UpvoteCount,
                    Anime = new Anime
                    {
                        Title = l.Anime.Title,
                        Year = l.Anime.Year,
                        MediaType = l.Anime.MediaType,
                        ImageUrl = l.Anime.ImageUrl
                    }
                }
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return library;
    }
}

public static class GetLibraryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/library/{userId}", async (
            [FromRoute] string userId, 
            GetLibraryHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetLibraryRequest(userId);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}
