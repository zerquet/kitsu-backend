using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Animes;
/// <summary>
/// Request to get animes for admin page.
/// </summary>
public record GetTopAnimeRequest;

public record CategoryItemResponse(int Id, string? Name, string? Description);

public class GetTopAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public GetTopAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<Anime>> Handle(GetTopAnimeRequest request, CancellationToken cancellationToken)
    {
        var animes = await _context.Animes
            .Select(a => new Anime
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Score = a.Score,
                Categories = a.Categories,
                ReleaseStatus = a.ReleaseStatus,
                ImageUrl = a.ImageUrl,
                Year = a.Year,
                CoverImageId = a.CoverImageId,
                EpisodeCount = a.EpisodeCount,
                MediaType = a.MediaType,
                EnglishTitle = a.EnglishTitle,
                JapaneseTitle = a.JapaneseTitle,
                JapaneseTitleRomaji = a.JapaneseTitleRomaji,
                Season = a.Season,
                StartAirDate = a.StartAirDate,
                EndAirDate = a.EndAirDate,
                TvRating = a.TvRating,
                EpisodeLength = a.EpisodeLength,
                FranchiseId = a.FranchiseId,
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return animes;
    }
}

public static class GetTopAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/animes", async (GetTopAnimeHandler handler, CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetTopAnimeRequest(), ct);

            return Results.Ok(result);
        });
    }
}