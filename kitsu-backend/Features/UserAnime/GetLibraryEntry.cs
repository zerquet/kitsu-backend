using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to get information for an anime if it is in the user's library or not.
/// </summary>
/// <param name="AnimeId"></param>
/// <param name="Email"></param>
public record GetLibraryEntryRequest(int AnimeId, string Email);
public class GetLibraryEntryResponse : GetLibraryResponse
{
    public ProfileResponse UserProfile { get; set; } = null!;

}
public class GetLibraryEntryHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public GetLibraryEntryHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager) 
    { 
        _context = context;
        _userManager = userManager;
    }

    public async Task<GetLibraryEntryResponse?> Handle(GetLibraryEntryRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        var libraryItem = await _context.LibraryEntries
            .Where(x => x.ApplicationUserId == user!.Id && x.AnimeId == request.AnimeId)
            .Select(le => new GetLibraryEntryResponse
            {
                LibraryEntryId = le.Id,
                AnimeId = le.AnimeId,
                EpisodesWatched = le.EpisodesWatched,
                UserRating = le.UserRating,
                WatchStatus = le.WatchStatus,
                Anime = new Anime
                {
                    Id = le.AnimeId,
                    Title = le.Anime.Title,
                    EpisodeCount = le.Anime.EpisodeCount,
                    ImageUrl = le.Anime.ImageUrl
                },
                UserProfile = new ProfileResponse
                {

                    Id = le.ApplicationUser.Id,
                    DisplayId = le.ApplicationUser.DisplayId,
                    Username = le.ApplicationUser.UserName,
                    Bio = le.ApplicationUser.Bio,
                    Location = le.ApplicationUser.Location,
                    Birthday = le.ApplicationUser.Birthday,
                    Gender = le.ApplicationUser.Gender,
                    ProfilePhotoId = le.ApplicationUser.ProfilePhotoId,
                    CoverPhotoId = le.ApplicationUser.CoverPhotoId
                },
                MediaReaction = le.MediaReaction == null ? null : new MediaReactionByUserResponse
                {
                    MediaReactionId = le.MediaReaction.Id,
                    UserId = le.MediaReaction.ApplicationUserId,
                    LibraryEntryId = le.Id,
                    AnimeId = le.MediaReaction.AnimeId,
                    Reaction = le.MediaReaction.Reaction,
                    UpvoteCount = le.MediaReaction.UpvoteCount,
                    Anime = new Anime
                    {
                        Title = le.Anime.Title,
                        Year = le.Anime.Year,
                        MediaType = le.Anime.MediaType,
                        ImageUrl = le.Anime.ImageUrl
                    }
                }
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (libraryItem != null)
            return libraryItem;

        // If the library entry does not exist, return the user's profile information without the library entry details.
        return new GetLibraryEntryResponse
        {
            UserProfile = new ProfileResponse
            {
                Id = user.Id,
                DisplayId = user.DisplayId,
                Username = user.UserName,
                Bio = user.Bio,
                Location = user.Location,
                Birthday = user.Birthday,
                Gender = user.Gender,
                ProfilePhotoId = user.ProfilePhotoId,
                CoverPhotoId = user.CoverPhotoId
            }
        };
    }
}

public static class GetLibraryEntryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/libraryEntry/{animeId}", [Authorize] async (
            [FromRoute] int animeId, 
            GetLibraryEntryHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.NotFound();

            var query = new GetLibraryEntryRequest(animeId, email);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}
