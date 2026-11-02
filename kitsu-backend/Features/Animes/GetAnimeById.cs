using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace kitsu_backend.Features.Animes;
/// <summary>
/// Request for an anime for the anime page. 
/// </summary>
/// <param name="Id">Id of the anime</param>
public record GetAnimeByIdRequest(int Id, string? Email);
public class GetAnimeByIdResponse
{
    public Anime Anime { get; set; } = null!; //Do not return entity, instead return DTO.
    public bool IsFavorite { get; set; }
}
public class GetAnimeByIdHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public GetAnimeByIdHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager) {
        _context = context;
        _userManager = userManager;
    }

    public async Task<GetAnimeByIdResponse?> Handle(GetAnimeByIdRequest request, CancellationToken cancellationToken)
    {
        var anime = await _context.Animes
            .Where(a =>  a.Id == request.Id)
            .Select(a => new Anime
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Score = a.Score,
                ImageUrl = a.ImageUrl,
                Year = a.Year,
                MediaType = a.MediaType,
                ReleaseStatus = a.ReleaseStatus,
                CoverImageId = a.CoverImageId,
                EpisodeCount = a.EpisodeCount,
                EnglishTitle = a.EnglishTitle,
                JapaneseTitle = a.JapaneseTitle,
                JapaneseTitleRomaji = a.JapaneseTitleRomaji,
                Season = a.Season,
                StartAirDate = a.StartAirDate,
                EndAirDate = a.EndAirDate,
                TvRating = a.TvRating,
                EpisodeLength = a.EpisodeLength,
                FranchiseId = a.FranchiseId,
                Franchise = a.Franchise,
                Categories = a.Categories.Select(c => new Category
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                }).ToList(),
                
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (anime == null)
            return null;

        var animeResponse = new GetAnimeByIdResponse
        {
            Anime = anime
        };

        if (string.IsNullOrEmpty(request.Email))
            return animeResponse;

        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return animeResponse;

        var isFavorite = await _context.FavoriteAnimes
            .AnyAsync(fa => fa.AnimeId == request.Id && fa.ApplicationUserId == user.Id);

        animeResponse.IsFavorite = isFavorite;

        return animeResponse;
    }
}

public static class GetAnimeByIdEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes/{id}", async (
            [FromRoute] int id, 
            GetAnimeByIdHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            var query = new GetAnimeByIdRequest(id, email);

            var result = await handler.Handle(query, ct);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        });

    }
}
