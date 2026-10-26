using kitsu.Shared.Dtos;
using kitsu_backend.Entities;
using kitsu_backend.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace kitsu.Features.Users;
/// <summary>
/// Request to update the authenticated user's profile information.
/// </summary>
public class UpdateProfileRequest
{
    public string Id { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? Location { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? Gender { get; set; }
    public string Email { get; set; } = string.Empty;
    public IFormFile? ProfilePhoto { get; set; }
    public IFormFile? CoverPhoto { get; set; }
}
public class UpdateProfileHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    public UpdateProfileHandler(UserManager<ApplicationUser> userManager) => _userManager = userManager;

    public async Task<ProfileResponse?> Handle(UpdateProfileRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        user.Bio = request.Bio;
        user.Location = request.Location;
        user.Birthday = request.Birthday;
        user.Gender = request.Gender;

        if (request.ProfilePhoto != null)
        {
            var oldProfileId = user.ProfilePhotoId;
            var profileId = Guid.NewGuid().ToString();
            await FileServerService.PostAnimeImage(profileId, request.ProfilePhoto, MediaTypes.UserProfile);
            user.ProfilePhotoId = profileId;

            if (!string.IsNullOrEmpty(oldProfileId))
            {
                await FileServerService.DeleteImage(oldProfileId, MediaTypes.UserProfile);
            }
        }
        if (request.CoverPhoto != null)
        {
            var oldCoverId = user.CoverPhotoId;
            var coverId = Guid.NewGuid().ToString();
            await FileServerService.PostAnimeImage(coverId, request.CoverPhoto, MediaTypes.UserCover);
            user.CoverPhotoId = coverId;

            if (!string.IsNullOrEmpty(oldCoverId))
            {
                await FileServerService.DeleteImage(oldCoverId, MediaTypes.UserCover);
            }
        }

        await _userManager.UpdateAsync(user);

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

public static class UpdateProfileEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/users", [Authorize] async ( 
            [FromForm] UpdateProfileRequest request,
            UpdateProfileHandler handler,
            ClaimsPrincipal user,
            CancellationToken ct) =>
        {
            var email = user.FindFirst(ClaimTypes.Email)?.Value;

            if (email == null)
                return Results.NotFound();

            request.Email = email;

            var result = await handler.Handle(request, ct);

            return Results.Ok(result);
        }).DisableAntiforgery();
    }
}




