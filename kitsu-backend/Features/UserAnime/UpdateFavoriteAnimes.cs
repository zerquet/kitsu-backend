using System.Security.Claims;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to update the user's favorites.
/// </summary>
/// <param name="AnimeIds"></param>
/// <param name="Email"></param>
public record UpdateFavoriteAnimesRequest(List<int> AnimeIds, string Email);

public class UpdateFavoriteAnimesHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public UpdateFavoriteAnimesHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Handle(UpdateFavoriteAnimesRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user != null)
        {
            var currentFavoriteList = await _context.FavoriteAnimes
                .Where(fa => fa.ApplicationUserId == user.Id)
                .Select(fa => fa.AnimeId)
                .ToListAsync(cancellationToken: cancellationToken);

            var toAdd = request.AnimeIds.Except(currentFavoriteList).ToList(); //animes user wants added
            var toRemove = currentFavoriteList.Except(request.AnimeIds).ToList(); //exceptions are kept

            foreach (var id in toAdd)
            {
                var favoriteAnime = new FavoriteAnime
                {
                    ApplicationUserId = user.Id,
                    AnimeId = id,
                    DateAdded = DateTime.UtcNow
                };
                await _context.FavoriteAnimes.AddAsync(favoriteAnime, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            foreach (var id in toRemove)
            {
                var found = await _context.FavoriteAnimes
                    .SingleOrDefaultAsync(fa => fa.ApplicationUserId == user.Id && fa.AnimeId == id, cancellationToken: cancellationToken);
                if (found != null)
                {
                    _context.FavoriteAnimes.Remove(found);
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }
        }
    }
}

public static class UpdateFavoriteAnimesEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/favorites/updatebulk", [Authorize] async (
            [FromBody] List<int> animeIds,
            UpdateFavoriteAnimesHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.Unauthorized();

            var query = new UpdateFavoriteAnimesRequest(animeIds, email);

            await handler.Handle(query, ct);

            return Results.Ok();
        });
    }
}