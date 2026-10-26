using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Animes;
/// <summary>
/// Request to get list of anime by a particular category
/// </summary>
/// <param name="CategoryTitle">The category to filter</param>
public record GetAnimesByCategoryRequest(string CategoryTitle, List<int> SkipAnimes);
public class GetAnimesByCategoryHandler
{
    private readonly ApplicationDbContext _context;
    public GetAnimesByCategoryHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Anime>> Handle(GetAnimesByCategoryRequest request, CancellationToken cancellationToken)
    {
        var categoryMatch = await _context.Categories
            .SingleOrDefaultAsync(c => c.Name == request.CategoryTitle, cancellationToken: cancellationToken);

        if (categoryMatch == null)
            return [];
        var animes = await _context.AnimeCategories
            .Where(ac => ac.CategoryId == categoryMatch.Id && !request.SkipAnimes.Contains(ac.AnimeId))
            .Include(ac => ac.Anime)
            .Select(ac => ac.Anime)
            .ToListAsync(cancellationToken: cancellationToken);
        
        return animes;
    }
}

public static class GetAnimesByCategoryEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/{category}/animes", async (
            [FromRoute] string category, 
            string? skip, 
            GetAnimesByCategoryHandler handler,
            CancellationToken ct) =>
        {
            var skipList = skip == null ? [] : skip.Split(',').Select(int.Parse).ToList();
            var query = new GetAnimesByCategoryRequest(category, skipList);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}