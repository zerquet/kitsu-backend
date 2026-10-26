using kitsu_backend.Database;
using kitsu_backend.Entities;
using kitsu_backend.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace kitsu_backend.Features.Animes;

/// <summary>
/// Admin only. Creates an anime record to be saved to the database.
/// </summary>
public class CreateAnimeRequest
{
    public string Title { get; set; } = "No title";
    public string? Description { get; set; }
    public int? Score { get; set; }
    public List<int> Categories { get; set; } = [];
    public string? Status { get; set; }
    public IFormFile? Image { get; set; }
    public int? Year { get; set; }
    public IFormFile? CoverImage { get; set; }
    public int? EpisodeCount { get; set; }
    public string? MediaType { get; set; }
    public string? EnglishTitle { get; set; }
    public string? JapaneseTitle { get; set; }
    public string? JapaneseTitleRomaji { get; set; }
    public string? Season { get; set; }
    public DateOnly? StartAirDate { get; set; }
    public DateOnly? EndAirDate { get; set; }
    public string? TvRating { get; set; }
    public int? EpisodeLength { get; set; }
    public int? FranchiseId { get; set; }
}
public class CreateAnimeHandler
{
    private readonly ApplicationDbContext _context;

    public CreateAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<Anime> Handle(CreateAnimeRequest request, CancellationToken cancellationToken)
    {
        var imageId = null as string;
        var coverImageId = null as string;
        if (request.Image != null)
        {
            imageId = Guid.NewGuid().ToString();
            await FileServerService.PostAnimeImage(imageId, request.Image, MediaTypes.AnimePoster);
        }
        if (request.CoverImage != null)
        {
            coverImageId = Guid.NewGuid().ToString();
            await FileServerService.PostAnimeImage(coverImageId, request.CoverImage, MediaTypes.AnimeCover);
        }
        var anime = new Anime
        {
            Title = request.Title,
            Description = request.Description,
            Score = request.Score,
            ReleaseStatus = request.Status,
            ImageUrl = imageId,
            Year = request.Year,
            EpisodeCount = request.EpisodeCount,
            MediaType = request.MediaType,
            CoverImageId = coverImageId,
            EnglishTitle = request.EnglishTitle,
            JapaneseTitle = request.JapaneseTitle,
            JapaneseTitleRomaji = request.JapaneseTitleRomaji,
            Season = request.Season,
            StartAirDate = request.StartAirDate,
            EndAirDate = request.EndAirDate,
            TvRating = request.TvRating,
            EpisodeLength = request.EpisodeLength,
            FranchiseId = request.FranchiseId,
        };

        await _context.Animes.AddAsync(anime, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var categories = await _context.Categories
            .Where(x => request.Categories.Contains(x.Id))
            .ToListAsync(cancellationToken: cancellationToken); //wtf, use id?

        foreach (var c in categories)
        {
            var newInstance = new AnimeCategory
            {
                AnimeId = anime.Id,
                CategoryId = c.Id,
            };
            await _context.AnimeCategories.AddAsync(newInstance, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var created = await _context.Animes.Where(a => a.Id == anime.Id)
            .Include(a => a.Categories)
            .Include(a => a.Franchise)
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        created.ImageUrl += ".jpg";
        created.CoverImageId += ".jpg";

        return created;
    }
}


public static class CreateAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/animes", [Authorize(Roles = "Admin")] async (
            CreateAnimeRequest request, 
            CreateAnimeHandler handler,
            CancellationToken ct) =>
        {
            if (request == null) return Results.BadRequest();

            var result = await handler.Handle(request, ct);

            return Results.Created(result.Id.ToString(), result);
        }).DisableAntiforgery();
    }
}

