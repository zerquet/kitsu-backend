using System.Security.Claims;
using kitsu_backend.Database;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.MediaReactions;

public record DeleteMediaReactionRequest(int Id, string Email);

public class DeleteMediaReactionHandler
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteMediaReactionHandler(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<bool> Handle(DeleteMediaReactionRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return false;

        var reaction = await _context.MediaReactions
            .Where(m => m.ApplicationUserId == user.Id && m.Id == request.Id)
            .SingleOrDefaultAsync(cancellationToken);

        if (reaction == null)       
            return false;

        _context.MediaReactions.Remove(reaction);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public static class DeleteMediaReactionEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/mediaReactions/{id}", [Authorize] async (
            [FromRoute] int id,
            DeleteMediaReactionHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.Unauthorized();
            
            var query = new DeleteMediaReactionRequest(id, email);
            
            var result = await handler.Handle(query, ct);
            return result ? Results.NoContent() : Results.NotFound();
        });
    }
}