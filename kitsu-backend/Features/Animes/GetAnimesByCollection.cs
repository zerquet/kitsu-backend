using kitsu.Shared;
using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.Animes;

public record GetAnimesByCollectionRequest(string Collection, string? Category);

public class GetAnimesByCollectionHandler
{
    private readonly ApplicationDbContext _context;
    public GetAnimesByCollectionHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AnimeResponseItem>> Handle(GetAnimesByCollectionRequest request, CancellationToken cancellationToken)
    {
        var collection = request.Collection.ToLower();
        if (!Constants.CollectionKinds.Contains(collection))
            return [];

        IQueryable<Anime> animes = _context.Animes;
        if (!string.IsNullOrEmpty(request.Category))
            animes = animes.Where(a => a.AnimeCategories.Any(ac => ac.Category.Slug == request.Category));

        var query = collection switch
        {
            "highest-rated" => animes
                .AsNoTracking()
                .OrderByDescending(a => a.Score),
            "new" => animes
                .AsNoTracking()
                .OrderByDescending(a => a.StartAirDate),
            "most-popular" => animes
                .AsNoTracking()
                .OrderByDescending(a => a.AnimeLibraryEntries.Count),
            _ => animes.AsNoTracking()
        };

        return await query
            .Take(Constants.DefaultCollectionLimit)
            .Select(a => new AnimeResponseItem
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Score = a.Score,
                ImageUrl = a.ImageUrl,
                Year = a.Year
            })
            .ToListAsync(cancellationToken);
    }
}

public static class GetAnimesByCollectionEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes/explore/{collection}", async (
            [FromRoute] string collection,
            [FromQuery] string? category,
            GetAnimesByCollectionHandler handler,
            CancellationToken ct) =>
        {
            if (!Constants.CollectionKinds.Contains(collection.ToLower()))
                return Results.BadRequest(new { error = $"Invalid collection '{collection}'. Valid options: {string.Join(", ", Constants.CollectionKinds)}" });

            var query = new GetAnimesByCollectionRequest(collection, category);
            var result = await handler.Handle(query, ct);
            return Results.Ok(result);
        });
    }
}