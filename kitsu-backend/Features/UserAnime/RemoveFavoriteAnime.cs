using System.Security.Claims;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to remove an anime from the user's favorites.
/// </summary>
/// <param name="AnimeId"></param>
/// <param name="Email"></param>
public record RemoveFavoriteAnimeRequest(int AnimeId, string Email);

public class RemoveFavoriteAnimeHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public RemoveFavoriteAnimeHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Handle(RemoveFavoriteAnimeRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            var found = await _context.FavoriteAnimes
                .SingleOrDefaultAsync(fa => fa.ApplicationUserId == user.Id && fa.AnimeId == request.AnimeId);

            if (found != null)
            {
                _context.FavoriteAnimes.Remove(found);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

public static class RemoveFavoriteAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/favorites/{animeId}", [Authorize] async (
            [FromRoute] int animeId,
            RemoveFavoriteAnimeHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.Unauthorized();

            var query = new RemoveFavoriteAnimeRequest(animeId, email);

            await handler.Handle(query, ct);

            return Results.Ok();
        });
    }
}