using System.Security.Claims;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.UserAnime;
/// <summary>
/// Request to get list of all favorite animes of the user.
/// </summary>
/// <param name="Email"></param>
public record GetFavoriteAnimesRequest(string UserId);

public class GetFavoriteAnimesHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public GetFavoriteAnimesHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IEnumerable<FavoriteAnime>> Handle(GetFavoriteAnimesRequest request, CancellationToken cancellationToken)
    {
        var favoriteAnime = await
            (from fa in _context.FavoriteAnimes
             join u in _context.Users on fa.ApplicationUserId equals u.Id
             join a in _context.Animes on fa.AnimeId equals a.Id
             where u.DisplayId == request.UserId
             select new FavoriteAnime
             {
                 Anime = new Anime
                 {
                     Id = a.Id,
                     Title = a.Title,
                     ImageUrl = a.ImageUrl
                 },
                 AnimeId = fa.AnimeId,
                 ApplicationUserId = u.DisplayId
             }).ToListAsync(cancellationToken: cancellationToken);

        return favoriteAnime;
    }
}   

public static class GetFavoriteAnimesEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/{userId}/favorites", async (
            [FromRoute] string userId,
            GetFavoriteAnimesHandler handler, 
            CancellationToken ct) =>
        {
            var query = new GetFavoriteAnimesRequest(userId);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}