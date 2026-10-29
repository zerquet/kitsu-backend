using kitsu_backend.Database;
using kitsu_backend.Entities;
using kitsu_backend.Features.Admin;
using kitsu_backend.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.Admin;

/// <summary>
/// Admin only. Updates an anime record in database.
/// </summary>
//not sure why some of these are required. should be optional. null?...
public class UpdateAnimeRequest
{
    public int Id { get; set; }
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

public class UpdateAnimeResponse : AnimeTableResponseItem;

internal sealed class UpdateAnimeHandler
{
    private readonly ApplicationDbContext _context;
    public UpdateAnimeHandler(ApplicationDbContext context) => _context = context;
    public async Task<UpdateAnimeResponse?> Handle(UpdateAnimeRequest request, CancellationToken cancellationToken)
    {
        if (request.Categories == null)
            request.Categories = [];

        var animeMatch = await _context.Animes
            .Include(a => a.Categories)
            .Include(a => a.Franchise)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (animeMatch == null)
            return null;

        if (request.Image != null)
        {
            var oldimageId = animeMatch.ImageUrl;
            var imageId = await AzureStorageService.PostImage(request.Image, AzureStoragePaths.AnimePoster);
            animeMatch.ImageUrl = imageId;

            if (!string.IsNullOrEmpty(oldimageId))
            {
                await AzureStorageService.DeleteImage(oldimageId, AzureStoragePaths.AnimePoster);
            }
        }
        if (request.CoverImage != null)
        {
            var oldCoverImage = animeMatch.CoverImageId;
            var coverImageId = await AzureStorageService.PostImage(request.CoverImage, AzureStoragePaths.AnimeCover);
            animeMatch.CoverImageId = coverImageId;

            if (!string.IsNullOrEmpty(oldCoverImage))
            {
                await AzureStorageService.DeleteImage(oldCoverImage, AzureStoragePaths.AnimeCover);
            }
        }
        animeMatch.Title = request.Title;
        animeMatch.Description = request.Description;
        animeMatch.Score = request.Score;
        animeMatch.ReleaseStatus = request.ReleaseStatus;
        animeMatch.Year = request.Year;
        animeMatch.EpisodeCount = request.EpisodeCount;
        animeMatch.MediaType = request.MediaType;
        animeMatch.EnglishTitle = request.EnglishTitle;
        animeMatch.JapaneseTitle = request.JapaneseTitle;
        animeMatch.JapaneseTitleRomaji = request.JapaneseTitleRomaji;
        animeMatch.Season = request.Season;
        animeMatch.StartAirDate = request.StartAirDate;
        animeMatch.EndAirDate = request.EndAirDate;
        animeMatch.TvRating = request.TvRating;
        animeMatch.EpisodeLength = request.EpisodeLength;
        animeMatch.FranchiseId = request.FranchiseId;

        await UpdateCategories(animeMatch, request.Categories);

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateAnimeResponse
        {
            Id = animeMatch.Id,
            Title = animeMatch.Title,
            Score = animeMatch.Score,
            ReleaseStatus = animeMatch.ReleaseStatus,
            ImageUrl = animeMatch.ImageUrl,
            Year = animeMatch.Year,
            EpisodeCount = animeMatch.EpisodeCount,
            MediaType = animeMatch.MediaType
        };
    }

    private async Task UpdateCategories(Anime anime, List<int> newCategories)
    {
        var currentCateogies = anime.Categories.ToList();

        foreach (var existingCategory in currentCateogies)
        {
            if (!newCategories.Contains(existingCategory.Id))
            {
                anime.Categories.Remove(existingCategory);
            }
        }

        foreach (var newCategory in newCategories)
        {
            if (!anime.Categories.Select(c => c.Id).ToList().Contains(newCategory))
            {
                var newInstance = new AnimeCategory
                {
                    AnimeId = anime.Id,
                    CategoryId = newCategory
                };
                await _context.AnimeCategories.AddAsync(newInstance);
            }
        }
    }
}

public static class UpdateAnimeEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/admin/animes", [Authorize(Roles = "Admin")] async (
            [FromForm] UpdateAnimeRequest request,
            UpdateAnimeHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request, ct);

            return result == null 
                ? Results.NotFound() 
                : Results.Ok(result);

        }).DisableAntiforgery();
    }
}
