using System.Security.Claims;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to add an anime to the user's favorites list.
/// </summary>
/// <param name="AnimeId"></param>
/// <param name="Email"></param>
public record AddFavoriteAnimeRequest(int AnimeId, string Email);

internal sealed class AddFavoriteAnimeHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public AddFavoriteAnimeHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<FavoriteAnime?> Handle(AddFavoriteAnimeRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        var favoriteEntry = new FavoriteAnime
        {
            ApplicationUserId = user.Id,
            AnimeId = request.AnimeId,
            DateAdded = DateTime.UtcNow
        };
        await _context.FavoriteAnimes.AddAsync(favoriteEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.FavoriteAnimes
            .Where(fa => fa.AnimeId == request.AnimeId && fa.ApplicationUserId == user.Id)
            .Select(fa => new FavoriteAnime
            {
                AnimeId = fa.AnimeId,
                Anime = new Anime
                {
                    Id = fa.Anime.Id,
                    Title = fa.Anime.Title,
                    ImageUrl = fa.Anime.ImageUrl
                },
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return created;
    }
}

public static class AddFavoriteAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/favorites/{animeId}", [Authorize] async (
            [FromRoute] int animeId,
            AddFavoriteAnimeHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.NotFound();

            var query = new AddFavoriteAnimeRequest(animeId, email);

            var result = await handler.Handle(query, ct);

            return result is null ? Results.BadRequest("Failed to add favorite anime") : Results.Ok(result);
        });
    }
}