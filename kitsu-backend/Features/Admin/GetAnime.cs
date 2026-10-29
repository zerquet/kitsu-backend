using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Admin;
/// <summary>
/// Request to get animes for admin page.
/// </summary>
public record GetAnimeRequest;

public class AnimeTableResponseItem
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public int? Score { get; set; }
    public string? ReleaseStatus { get; set; }
    public string? ImageUrl { get; set; }
    public int? Year { get; set; }
    public int? EpisodeCount { get; set; }
    public string? MediaType { get; set; }
}
public class GetAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public GetAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<AnimeTableResponseItem>> Handle(GetAnimeRequest request, CancellationToken cancellationToken)
    {
        var animes = await _context.Animes
            .Select(a => new AnimeTableResponseItem
            {
                Id = a.Id,
                Title = a.Title,
                Score = a.Score,
                ReleaseStatus = a.ReleaseStatus,
                ImageUrl = a.ImageUrl,
                Year = a.Year,
                EpisodeCount = a.EpisodeCount,
                MediaType = a.MediaType,
            })
            .Take(30)
            .ToListAsync(cancellationToken: cancellationToken);

        return animes;
    }
}

public static class GetAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/animes", async (GetAnimeHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetAnimeRequest(), ct);

            return Results.Ok(result);
        });
    }
}