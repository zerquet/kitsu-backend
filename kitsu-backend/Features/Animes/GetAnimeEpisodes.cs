using kitsu_backend.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.Animes;
/// <summary>
/// Request to get a list of episodes by anime
/// </summary>
/// <param name="AnimeId"></param>
public record GetAnimeEpisodesRequest(int AnimeId);
public record EpisodeInfoResponse(int? Number, string? Title, DateOnly? AirDate);

internal sealed class GetAnimeEpisodesHandler
{
    private readonly ApplicationDbContext _context;
    public GetAnimeEpisodesHandler(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<EpisodeInfoResponse>> Handle(GetAnimeEpisodesRequest request, CancellationToken cancellationToken)
    {
        var episodes = await _context.Episodes
            .Where(e => e.AnimeId == request.AnimeId)
            .Select(e => new EpisodeInfoResponse(e.Number, e.Title, e.AirDate))
            .ToListAsync(cancellationToken: cancellationToken);

        return episodes;
    }
}

public static class GetAnimeEpisodesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/anime/{animeId:int}/episodes", async (
            [FromRoute] int animeId, 
            GetAnimeEpisodesHandler handler, 
            CancellationToken ct) =>
        {
            var query = new GetAnimeEpisodesRequest(animeId);

            var result = await handler.Handle(query, ct);

            return Results.Ok(result);
        });
    }
}
