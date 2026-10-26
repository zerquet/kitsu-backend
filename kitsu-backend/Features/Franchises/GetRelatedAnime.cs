using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.Franchises;
/// <summary>
/// Request to get list of anime that are a part of a franchise
/// </summary>
/// <param name="FranchiseId">Id of franchise</param>
public record GetRelatedAnimeRequest(int FranchiseId, List<int> SkipAnimes);

public class GetRelatedAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public GetRelatedAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Anime>> Handle(GetRelatedAnimeRequest request, CancellationToken cancellationToken)
    {
        var animes = await _context.Animes
                .Where(a => a.FranchiseId == request.FranchiseId && !request.SkipAnimes.Contains(a.Id))
                .Include(a => a.Franchise)
                .ToListAsync(cancellationToken: cancellationToken);

        return animes;
    }
}

public static class GetRelatedAnimeEndpoint //maybe this should be in franchise feature? is the route string functional?
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/franchises/{franchiseId}/animes", async (
            [FromRoute] int franchiseId, 
            string? skip, 
            GetRelatedAnimeHandler handler,
            CancellationToken ct) =>
        {
            var skipList = skip == null ? [] : skip.Split(',').Select(int.Parse).ToList();
            var query = new GetRelatedAnimeRequest(franchiseId, skipList);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}