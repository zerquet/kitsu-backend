using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace kitsu.Features.MediaReactions;

public class AddMediaReactionRequest(int animeId, int libraryEntryId, string reaction)
{
    public int AnimeId { get; set; } = animeId;
    public int LibraryEntryId { get; set; } = libraryEntryId;
    public string? Email { get; set; }
    [StringLength(maximumLength: 140)]
    public string Reaction { get; set; } = reaction;
}

public class AddMediaReactionResponse : MediaReactionByUserResponse { }

public class AddMediaReactionHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AddMediaReactionHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<AddMediaReactionResponse?> Handle(AddMediaReactionRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        var exists = await _context.MediaReactions
            .AnyAsync(mr => mr.ApplicationUserId == user.Id
                && mr.AnimeId == request.AnimeId
                && mr.LibraryEntryId == request.LibraryEntryId, cancellationToken);

        if (exists)
            return null;

        var mediaReaction = new MediaReaction
        {
            ApplicationUserId = user.Id,
            AnimeId = request.AnimeId,
            LibraryEntryId = request.LibraryEntryId,
            Reaction = request.Reaction,
            UpvoteCount = 0,
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow,
        };
        
        await _context.MediaReactions.AddAsync(mediaReaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await (from r in _context.MediaReactions
                             join a in _context.Animes on r.AnimeId equals a.Id
                             where r.Id == mediaReaction.Id 
                             select new AddMediaReactionResponse
                             {
                                 MediaReactionId = r.Id,
                                 UserId = r.ApplicationUserId,
                                 Reaction = r.Reaction,
                                 UpvoteCount = r.UpvoteCount,
                                 AnimeId = r.AnimeId,
                                 Anime = new Anime
                                 {
                                    Title = a.Title,
                                    Year = a.Year,
                                    MediaType = a.MediaType,
                                    ImageUrl = $"{a.ImageUrl}.jpg",
                                 },
                                 LibraryEntryId = r.LibraryEntryId,
                             }).SingleOrDefaultAsync(cancellationToken);

        return created;
    }
}

public static class AddMediaReactionEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/mediaReactions", [Authorize] async (
            [FromBody] AddMediaReactionRequest request,
            AddMediaReactionHandler handler, 
            ClaimsPrincipal user,
            CancellationToken ct
            ) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.Unauthorized();

            request.Email = email;

            var result = await handler.Handle(request, ct);

            return result is null 
                ? Results.BadRequest("Unauthorized or media reaction already exists for this library entry")
                : Results.Created(result.MediaReactionId.ToString(), result);
        }).DisableAntiforgery();
    }
}