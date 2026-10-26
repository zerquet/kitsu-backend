using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq.Expressions;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to create a library entry. When a user adds an anime to their watching/planning/completed lists.
/// </summary>
/// <param name="AnimeId"></param>
/// <param name="WatchStatus"></param>
/// <param name="EpisodesWatched"></param>
/// <param name="Email"></param>
public record AddToLibraryRequest(int AnimeId, string WatchStatus, int EpisodesWatched, string Email)
{
    public AddToLibraryRequest() : this(0, "", 0, "")
    {
    }
}
public class AddToLibraryResponse : GetLibraryResponse { }
public class AddToLibraryHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public AddToLibraryHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AddToLibraryResponse?> Handle(AddToLibraryRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        //check if entry already exists for this user and anime
        var exists = await _context.LibraryEntries
            .AnyAsync(le => le.ApplicationUserId == user.Id && le.AnimeId == request.AnimeId, cancellationToken);

        if (exists)
        {
            var existingEntry = await _context.LibraryEntries
                .Where(le => le.ApplicationUserId == user.Id && le.AnimeId == request.AnimeId)
                .Select(ToDtoProjection())
                .SingleOrDefaultAsync(cancellationToken: cancellationToken);

            return existingEntry;
        }

        var libraryEntry = new LibraryEntry
        {
            ApplicationUserId = user.Id,
            ApplicationUserDisplayId = user.DisplayId,
            AnimeId = request.AnimeId,
            WatchStatus = request.WatchStatus,
            EpisodesWatched = request.EpisodesWatched
        };
        await _context.LibraryEntries.AddAsync(libraryEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.LibraryEntries.Where(le => le.Id == libraryEntry.Id)
            .Include(le => le.Anime)
            .Select(ToDtoProjection())
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return created;

    }

    private static Expression<Func<LibraryEntry, AddToLibraryResponse>> ToDtoProjection()
    {
        return le => new AddToLibraryResponse
        {
            LibraryEntryId = le.Id,
            AnimeId = le.AnimeId,
            WatchStatus = le.WatchStatus,
            UserRating = le.UserRating,
            EpisodesWatched = le.EpisodesWatched,
            MediaReaction = null, // no media reaction when created
            Anime = new Anime
            {
                Id = le.AnimeId,
                Title = le.Anime.Title,
                EpisodeCount = le.Anime.EpisodeCount
            },
        };
    }
}

public static class AddToLibraryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/library", [Authorize]
        async (
            [FromBody] AddToLibraryRequest request,
            AddToLibraryHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct
            ) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.NotFound();

            //maybe this is why its good to have a different model for the request in the endpoint and the handler.
            //that way, we dont have to do this ugly code below of re-creating another instance of the same class.
            var query = new AddToLibraryRequest(request.AnimeId, request.WatchStatus, request.EpisodesWatched, email);

            var result = await handler.Handle(query, ct);

            return result is null
                ? Results.BadRequest("Unauthorized or something went wrong")
                : Results.Ok(result);
        });
    }
}
