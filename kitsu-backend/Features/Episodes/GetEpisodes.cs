using kitsu_backend.Database;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Episodes;
/// <summary>
/// Request to get a list of episodes that are in the database.
/// </summary>
public record GetEpisodesRequest();
public record EpisodeBriefResponse(int Id, int? Number, string? Title);

public class GetEpisodesHandler
{
    private readonly ApplicationDbContext _context;
    public GetEpisodesHandler(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<EpisodeBriefResponse>> Handle(GetEpisodesRequest request, CancellationToken cancellationToken)
    {
        var episodes = await _context.Episodes.Select(e => new EpisodeBriefResponse(e.Id, e.Number, e.Title))
            .ToListAsync(cancellationToken: cancellationToken);

        return episodes;
    }
}

public static class GetEpisodesEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/episodes", async (
            GetEpisodesHandler handler, 
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new GetEpisodesRequest(), ct);

            return Results.Ok(result);
        });
    }
}