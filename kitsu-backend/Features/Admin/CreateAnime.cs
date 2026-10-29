using kitsu_backend.Database;
using kitsu_backend.Entities;
using kitsu_backend.Features.Admin;
using kitsu_backend.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.Admin;

/// <summary>
/// Admin only. Creates an anime record to be saved to the database.
/// </summary>
public class CreateAnimeRequest
{
    public string Title { get; set; } = "No title";
    public string? Description { get; set; }
    public int? Score { get; set; }
    public List<int> Categories { get; set; } = [];
    public string? ReleaseStatus { get; set; }
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

public class CreateAnimeResponse : AnimeTableResponseItem;
public class CreateAnimeHandler
{
    private readonly ApplicationDbContext _context;

    public CreateAnimeHandler(ApplicationDbContext context) => _context = context;

    public async Task<CreateAnimeResponse?> Handle(CreateAnimeRequest request, CancellationToken cancellationToken)
    {
        var imageId = null as string;
        var coverImageId = null as string;
        if (request.Image != null)
        {
            imageId = await AzureStorageService.PostImage(request.Image, AzureStoragePaths.AnimePoster);
        }
        if (request.CoverImage != null)
        {
            coverImageId = await AzureStorageService.PostImage(request.CoverImage, AzureStoragePaths.AnimeCover);
        }
        var anime = new Anime
        {
            Title = request.Title,
            Description = request.Description,
            Score = request.Score,
            ReleaseStatus = request.ReleaseStatus,
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
            .Select(a => new CreateAnimeResponse
            {
                Id = a.Id,
                Title = a.Title,
                ReleaseStatus = a.ReleaseStatus,
                EpisodeCount = a.EpisodeCount,
                ImageUrl = a.ImageUrl,
                MediaType = a.MediaType,
                Year = a.Year,
                Score = a.Score
            })
            .SingleOrDefaultAsync(cancellationToken: cancellationToken);

        return created;
    }
}


public static class CreateAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/animes", [Authorize(Roles = "Admin")] async (
            [FromForm] CreateAnimeRequest request,
            CreateAnimeHandler handler,
            CancellationToken ct) =>
        {
            if (request == null) return Results.BadRequest();

            var result = await handler.Handle(request, ct);

            return result == null
                ? Results.NotFound()
                : Results.Ok(result);
        }).DisableAntiforgery();
    }
}

