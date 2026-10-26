using System.Security.Claims;
using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.MediaReactions;

public record GetMediaReactionsByUserRequest(string UserId);

public class GetMediaReactionsByUserHandler
{
    private readonly ApplicationDbContext _context;
    public GetMediaReactionsByUserHandler(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<MediaReactionByUserResponse>> Handle(GetMediaReactionsByUserRequest request, CancellationToken cancellationToken)
    {
        var mediaReactions = await (from mr in _context.MediaReactions
            join u in _context.Users on mr.ApplicationUserId equals u.Id
            join a in _context.Animes on mr.AnimeId equals a.Id
            where u.DisplayId == request.UserId
            select new MediaReactionByUserResponse {
                MediaReactionId = mr.Id,
                UserId = u.DisplayId,
                LibraryEntryId = mr.LibraryEntryId,
                AnimeId = mr.AnimeId,
                Reaction = mr.Reaction,
                UpvoteCount = mr.UpvoteCount,
                Anime = new Anime
                {
                    Title = a.Title,
                    Year = a.Year,
                    MediaType = a.MediaType,
                    ImageUrl = a.ImageUrl
                }
            }).ToListAsync(cancellationToken: cancellationToken);

        return mediaReactions;
    }
}

public static class GetMediaReactionsByUserEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/{userId}/mediaReactions", async (
            [FromRoute] string userId,
            GetMediaReactionsByUserHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetMediaReactionsByUserRequest(userId);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}

