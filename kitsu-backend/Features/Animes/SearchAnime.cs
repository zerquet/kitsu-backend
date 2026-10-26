using kitsu.Shared;
using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Animes;
/// <summary>
/// Request to get list of animes for the advanced search page.
/// </summary>
/// <param name="Term"></param>
/// <param name="MinYear"></param>
/// <param name="MaxYear"></param>
/// <param name="MinEpisodes"></param>
/// <param name="MaxEpisodes"></param>
/// <param name="MinRating"></param>
/// <param name="MaxRating"></param>
/// <param name="MediaType"></param>
public record SearchAnimeRequest
(
    string? Term,
    int? MinYear,
    int? MaxYear,
    int? MinEpisodes,
    int? MaxEpisodes,
    int? MinRating,
    int? MaxRating,
    string? MediaType
);

public class AnimeSearchResultResponse : AnimeResponseItem;

public class SearchAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public SearchAnimeHandler(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<AnimeSearchResultResponse>> Handle(SearchAnimeRequest request, CancellationToken cancellationToken)
    {
        var mediaTypes = request.MediaType?.Split(',');

        // The frontend slider tops out at 100, which means "100+" (no upper bound).
        var minEpisodes = request.MinEpisodes ?? 1;
        var maxEpisodes = request.MaxEpisodes ?? 100;
        var noMaxEpisodes = maxEpisodes >= 100;

        var animes = await _context.Animes
                .AsNoTracking()
                .Where(a =>
                    (request.Term == null || a.Title.Contains(request.Term)) &&
                    a.Year >= request.MinYear && a.Year <= request.MaxYear &&
                    // Unknown episode count (e.g. long-running series) only matches when there's no upper bound.
                    (a.EpisodeCount == null
                        ? noMaxEpisodes
                        : a.EpisodeCount >= minEpisodes && (noMaxEpisodes || a.EpisodeCount <= maxEpisodes)) &&
                    a.Score >= request.MinRating && a.Score <= request.MaxRating &&
                    (mediaTypes == null || mediaTypes.Contains(a.MediaType))
                    )
                .Select(a => new AnimeSearchResultResponse
                { 
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Score = a.Score,
                    Year = a.Year,
                    ImageUrl = a.ImageUrl
                })
                .Take(30)
                .ToListAsync(cancellationToken: cancellationToken);

        return animes;
    }
}

public static class SearchAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes/search", async (
            [FromQuery] string? term,
            [FromQuery] int? minYear,
            [FromQuery] int? maxYear,
            [FromQuery] int? minEpisodes,
            [FromQuery] int? maxEpisodes,
            [FromQuery] int? minRating,
            [FromQuery] int? maxRating,
            [FromQuery] string? mediaType,
            SearchAnimeHandler handler, 
            CancellationToken ct) =>
        {
            var query = new SearchAnimeRequest(term, minYear, maxYear, minEpisodes, maxEpisodes, minRating, maxRating, mediaType);

            var result = await handler.Handle(query, ct);

            return result;
        });
    }
}
