using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace kitsu_backend.Features.Admin;

/// <summary>
/// Admin only. Request for an anime entry. 
/// </summary>
/// <param name="Id">Id of the anime</param>
public record GetAnimeByIdRequest(int Id);
public class AdminAnimeDetailsResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = "No title";
    public string? Description { get; set; }
    public int? Score { get; set; }
    public List<Category> Categories { get; set; } = [];
    public string? ReleaseStatus { get; set; }
    public string? ImageUrl { get; set; }
    public int? Year { get; set; }
    public string? CoverImageId { get; set; }
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
    public Franchise? Franchise { get; set; }
}
public class GetAnimeByIdHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    public GetAnimeByIdHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AdminAnimeDetailsResponse?> Handle(GetAnimeByIdRequest request, CancellationToken cancellationToken)
    {
        var anime = await _context.Animes
            .Where(a => a.Id == request.Id)
            .Select(a => new AdminAnimeDetailsResponse
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                Score = a.Score,
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
                Franchise = a.Franchise != null ? new Franchise { Id = a.Franchise.Id, Name = a.Franchise.Name } : null,
                Categories = a.Categories.Select(c => new Category { Id = c.Id }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        return anime;
    }
}

public static class GetAnimeByIdEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/admin/animes/{id}", [Authorize(Roles = "Admin")] async (
            [FromRoute] int id,
            GetAnimeByIdHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetAnimeByIdRequest(id);

            var result = await handler.Handle(query, ct);

            return result is null
                ? Results.NotFound()
                : Results.Ok(result);
        });

    }
}