using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to delete an anime from the user's watch lists. 
/// </summary>
/// <param name="AnimeId"></param>
/// <param name="Email"></param>
public record RemoveFromLibraryRequest(int AnimeId, string Email)
{
    public RemoveFromLibraryRequest() : this(0, "") {}
}

public class RemoveFromLibraryHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public RemoveFromLibraryHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Handle(RemoveFromLibraryRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            var libraryEntry = await _context.LibraryEntries
                .Include(l => l.MediaReaction)
                .SingleOrDefaultAsync(x => x.ApplicationUserId == user.Id && x.AnimeId == request.AnimeId);
            if (libraryEntry != null)
            {
                if (libraryEntry.MediaReaction != null)
                    _context.MediaReactions.Remove(libraryEntry.MediaReaction);

                _context.LibraryEntries.Remove(libraryEntry);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

public static class RemoveFromLibraryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/library/{animeId}", [Authorize] async (
            [FromRoute] int animeId,
            RemoveFromLibraryHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.NotFound();

            var query = new RemoveFromLibraryRequest(animeId, email);

            await handler.Handle(query, ct);

            return Results.NoContent();
        });
    }
}
