using kitsu.Shared;
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
/// Request to make a change to an anime entry in the user's library.
/// </summary>
/// <param name="Id"></param>
/// <param name="AnimeId"></param>
/// <param name="UserRating"></param>
/// <param name="WatchStatus"></param>
/// <param name="EpisodesWatched"></param>
/// <param name="Email"></param>
public record UpdateLibraryEntryRequest(int LibraryEntryId, int AnimeId, int? UserRating, string WatchStatus, int EpisodesWatched, string Email)
{
    public UpdateLibraryEntryRequest() : this(0, 0, null, "", 0, "")
    {
    }
}
public class UpdateLibraryEntryResponse : GetLibraryResponse { }

public class UpdateLibraryEntryHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public UpdateLibraryEntryHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<UpdateLibraryEntryResponse?> Handle(UpdateLibraryEntryRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        var libraryEntry = await _context.LibraryEntries
            .Where(le => le.Id == request.LibraryEntryId && le.ApplicationUserId == user.Id)
            .Include(le => le.Anime)
            .Include(le => le.MediaReaction)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        if (libraryEntry == null)
            return null;

        var animeEpisodeCount = libraryEntry.Anime.EpisodeCount ?? Constants.MaxEpisodeProgress;

        var validEpisodesWatched = GetValidEpisodesWatched(request.EpisodesWatched, libraryEntry.EpisodesWatched, animeEpisodeCount);

        var validWatchStatus = GetValidWatchStatus(request.WatchStatus, validEpisodesWatched, animeEpisodeCount);

        libraryEntry.WatchStatus = validWatchStatus;
        libraryEntry.EpisodesWatched = validEpisodesWatched;
        libraryEntry.UserRating = GetValidUserRating(request.UserRating, libraryEntry.UserRating);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateLibraryEntryResponse
        {
            LibraryEntryId = libraryEntry.Id,
            AnimeId = libraryEntry.AnimeId,
            EpisodesWatched = libraryEntry.EpisodesWatched,
            UserRating = libraryEntry.UserRating,
            WatchStatus = libraryEntry.WatchStatus,
            Anime = new Anime
            {
                Id = libraryEntry.AnimeId,
                Title = libraryEntry.Anime.Title,
                EpisodeCount = libraryEntry.Anime.EpisodeCount,
                ImageUrl = libraryEntry.Anime.ImageUrl
            },
            MediaReaction = libraryEntry.MediaReaction == null ? null : new MediaReactionByUserResponse
            {
                MediaReactionId = libraryEntry.MediaReaction.Id,
                UserId = libraryEntry.MediaReaction.ApplicationUserId,
                LibraryEntryId = libraryEntry.Id,
                AnimeId = libraryEntry.MediaReaction.AnimeId,
                Reaction = libraryEntry.MediaReaction.Reaction,
                UpvoteCount = libraryEntry.MediaReaction.UpvoteCount,
                Anime = new Anime
                {
                    Title = libraryEntry.Anime.Title,
                    Year = libraryEntry.Anime.Year,
                    MediaType = libraryEntry.Anime.MediaType,
                    ImageUrl = $"{libraryEntry.Anime.ImageUrl}.jpg"
                }
            }
        };

    }

    internal static int GetValidEpisodesWatched(int episodesWatched, int currentEpisodesWatched, int animeEpisodeCount)
    {
        if (episodesWatched >= 0 && episodesWatched <= animeEpisodeCount) 
            return episodesWatched;
        return currentEpisodesWatched;
    }

    internal static int? GetValidUserRating(int? userRating, int? currentUserRating)
    {
        // null or 0 removes the rating
        if (userRating == null || userRating == 0)
            return null;
        if (userRating >= Constants.MinUserRating && userRating <= Constants.MaxUserRating)
            return userRating;
        return currentUserRating;
    }

    internal static string GetValidWatchStatus(string watchStatus, int episodeProgress, int animeEpisodeCount)
    {
        if (watchStatus == "completed" && episodeProgress != animeEpisodeCount)
            return "watching";
        else if (watchStatus == "watching" && episodeProgress == animeEpisodeCount)
            return "completed";
        else if (watchStatus == "planning" && episodeProgress > 0 && episodeProgress < animeEpisodeCount)
            return "watching";
        else if (watchStatus == "planning" && episodeProgress == animeEpisodeCount)
            return "completed";
        else
            return watchStatus;
    }
}

public static class UpdateLibraryEntryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/library", [Authorize]
        async (
            [FromBody] UpdateLibraryEntryRequest request,
            UpdateLibraryEntryHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct
            ) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.NotFound();

            var query = new UpdateLibraryEntryRequest
                (request.LibraryEntryId, request.AnimeId, request.UserRating, request.WatchStatus, request.EpisodesWatched, email);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}