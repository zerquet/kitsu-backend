using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using kitsu_backend.Features.Animes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace kitsu.Features.Animes;

public record GetExploreAnimeRequest(string? Category);

public class GetExploreAnimeResponse
{
    public IEnumerable<AnimeResponseItem> HighestRatedAnime { get; set; } = null!;
    public IEnumerable<AnimeResponseItem> NewAnime { get; set; } = null!;
    public IEnumerable<AnimeResponseItem> PopularAnime { get; set; } = null!;
}

public class GetExploreAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public GetExploreAnimeHandler(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<GetExploreAnimeResponse> Handle(GetExploreAnimeRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Anime> animes = _context.Animes;
        if (!string.IsNullOrEmpty(request.Category))
            animes = animes.Where(a => a.AnimeCategories.Any(ac => ac.Category.Slug == request.Category));

        var highestRatedAnime = await animes
            .OrderByDescending(a => a.Score).ThenBy(a => a.Id)
            .Take(6)
            .Select(ToDtoProjection()).ToListAsync(cancellationToken: cancellationToken);

        var newAnime = await animes
            .OrderByDescending(a => a.StartAirDate).ThenBy(a => a.Id)
            .Take(6)
            .Select(ToDtoProjection()).ToListAsync(cancellationToken: cancellationToken);

        var popularAnime = await animes
            .OrderByDescending(a => a.AnimeLibraryEntries.Count).ThenBy(a => a.Id)
            .Take(6)
            .Select(ToDtoProjection()).ToListAsync(cancellationToken: cancellationToken);

        var response = new GetExploreAnimeResponse
        {
            HighestRatedAnime = highestRatedAnime,
            NewAnime = newAnime,
            PopularAnime = popularAnime
        };

        return response;
    }

    private static Expression<Func<Anime, AnimeResponseItem>> ToDtoProjection()
    {
        return a => new AnimeResponseItem
        {
            Id = a.Id,
            Title = a.Title,
            Description = a.Description,
            Score = a.Score,
            ImageUrl = a.ImageUrl,
            Year = a.Year
        };
    }
}

public static class GetExploreAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes/explore", async ([FromQuery] string? category, GetExploreAnimeHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetExploreAnimeRequest(category), ct);

            return Results.Ok(result);
        });
    }
}