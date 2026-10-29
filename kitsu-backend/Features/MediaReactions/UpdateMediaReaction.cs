using System.Security.Claims;
using kitsu.Shared.Dtos;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.MediaReactions;

public record UpdateMediaReactionRequest(int ReactionId, string Email, string Reaction);

public class UpdateMediaReactionResponse : MediaReactionByUserResponse { }

public class UpdateMediaReactionHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public UpdateMediaReactionHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }
    public async Task<UpdateMediaReactionResponse?> Handle(UpdateMediaReactionRequest request, CancellationToken cancellationToken)
    {
        //Prevents unauthorized updates by checking userId
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        var reaction = await _context.MediaReactions
            .Where(m => m.ApplicationUserId == user.Id && m.Id == request.ReactionId)
            .Include(m => m.Anime)
            .SingleOrDefaultAsync(cancellationToken);

        if (reaction == null)
            return null;
        
        reaction.Reaction = request.Reaction;
        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateMediaReactionResponse
        {
            MediaReactionId = reaction.Id,
            UserId = reaction.ApplicationUserId,
            LibraryEntryId = reaction.LibraryEntryId,
            AnimeId = reaction.AnimeId,
            Reaction = reaction.Reaction,
            UpvoteCount = reaction.UpvoteCount,
            Anime = new Anime
            {
                Title = reaction.Anime.Title,
                Year = reaction.Anime.Year,
                MediaType = reaction.Anime.MediaType,
                ImageUrl = reaction.Anime.ImageUrl,
            }
        };
    }
}

public static class UpdateMediaReactionEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/mediaReactions", [Authorize] async (
            [FromBody] UpdateMediaReactionRequest request, 
            UpdateMediaReactionHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.Unauthorized();

            var query = new UpdateMediaReactionRequest(request.ReactionId, email, request.Reaction);

            var result = await handler.Handle(query, ct);
            return Results.Ok(result);
        });
    }
}
