using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Admin;
/// <summary>
/// Request to get animes for admin page.
/// </summary>
public record SearchAnimeRequest(string? Term);

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
public class SearchAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public SearchAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<AnimeTableResponseItem>> Handle(SearchAnimeRequest request, CancellationToken cancellationToken)
    {
        IQueryable<Anime> animes = _context.Animes;
        if (string.IsNullOrEmpty(request.Term))
            animes = animes.Where(a => a.StartAirDate != null).OrderByDescending(a => a.StartAirDate);
        else
            animes = animes.Where(a => a.Title.Contains(request.Term) || (a.EnglishTitle != null && a.EnglishTitle.Contains(request.Term)));

        var results = await animes
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

        return results;
    }
}

public static class SearchAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/animes/search", async (
            [FromQuery] string? term, 
            SearchAnimeHandler handler, 
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new SearchAnimeRequest(term), ct);

            return Results.Ok(result);
        });
    }
}