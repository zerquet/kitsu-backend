using kitsu.Shared.Dtos;
using kitsu_backend.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace kitsu.Features.Users;
/// <summary>
/// Request to get a user's profile information.
/// </summary>
/// <param name="UserId"></param>
public record GetProfileRequest(string UserId);

public class GetProfileHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    public GetProfileHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<ProfileResponse?> Handle(GetProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.DisplayId == request.UserId, cancellationToken);

        if (user == null) 
            return null;

        return new ProfileResponse
        {
            Id = user.Id,
            DisplayId = user.DisplayId,
            Username = user.UserName,
            Bio = user.Bio,
            Location = user.Location,
            Birthday = user.Birthday,
            Gender = user.Gender,
            ProfilePhotoId = user.ProfilePhotoId,
            CoverPhotoId = user.CoverPhotoId
        };
    }
}

public static class GetProfileEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{userId}", async (
            [FromRoute] string userId, 
            GetProfileHandler handler,
            CancellationToken ct) =>
        {
            var query = new GetProfileRequest(userId);

            var result = await handler.Handle(query, ct);

            return result is null 
                ? Results.NotFound() 
                : Results.Ok(result);
        });
    }
}