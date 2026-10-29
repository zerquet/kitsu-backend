using kitsu_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Animes;
/// <summary>
/// Request to retrieve a list of anime for small search functions in frontend.
/// </summary>
/// <param name="term">Term to search by</param>
public record GetAnimeByTitleRequest(string term);
public record AnimeBriefResponse(int Id, string Title, string ImageUrl, string MediaType);

public class GetAnimeByTitleHandler
{
    private readonly ApplicationDbContext _context;
    public GetAnimeByTitleHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<AnimeBriefResponse>> Handle(GetAnimeByTitleRequest request, CancellationToken cancellationToken)
    {
        if(string.IsNullOrEmpty(request.term))
            return [];

        var animes = await _context.Animes
                .AsNoTracking()
                .Where(a => a.Title.Contains(request.term) || (a.EnglishTitle != null && a.EnglishTitle.Contains(request.term)))
                .Take(4)
                .Select(a => new AnimeBriefResponse(a.Id, a.Title, a.ImageUrl, a.MediaType))
                .ToListAsync(cancellationToken: cancellationToken);

        return animes;
    }
}

public static class GetAnimeByTitleEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes/byTitle", async (
            [FromQuery] string term, 
            GetAnimeByTitleHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetAnimeByTitleRequest(term);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}
