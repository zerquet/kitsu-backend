using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace kitsu_backend.Features.Episodes;
/// <summary>
/// Admin only. Request to create an episode record for an anime
/// </summary>
/// <param name="AnimeId"></param>
/// <param name="Number"></param>
/// <param name="Title"></param>
/// <param name="Description"></param>
/// <param name="AirDate"></param>
/// <param name="JapaneseTitle"></param>
public record CreateEpisodeRequest
(
    int AnimeId,
    int? Number,
    string? Title,
    string? Description,
    DateTime? AirDate,
    string? JapaneseTitle
);

internal sealed class CreateEpisodeHandler
{
    private readonly ApplicationDbContext _context;
    public CreateEpisodeHandler(ApplicationDbContext context) => _context = context;
    public async Task Handle(CreateEpisodeRequest request, CancellationToken cancellationToken)
    {
        var anime = await _context.Animes.FindAsync([request.AnimeId], cancellationToken: cancellationToken);

        if (anime != null)
        {
            var episode = new Episode
            {
                AnimeId = anime.Id,
                Number = request.Number,
                Title = request.Title,
                Description = request.Description,
                JapaneseTitle = request.JapaneseTitle
            };

            if (request.AirDate != null)
            {
                var date = (DateTime)request.AirDate;
                episode.AirDate = new DateOnly(date.Year, date.Month, date.Day);
            }

            await _context.Episodes.AddAsync(episode, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

public static class CreateEpisodeEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/episodes", [Authorize(Roles = "Admin")] async (
            [FromBody] CreateEpisodeRequest request, 
            CreateEpisodeHandler handler, 
            CancellationToken ct) =>
        {
            await handler.Handle(request, ct);

            return Results.Created();
        });
    }
}