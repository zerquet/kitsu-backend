using kitsu_backend.Entities;
using kitsu_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace kitsu_backend.Features.Authentication;
/// <summary>
/// Request to create a new user.
/// </summary>
/// <param name="Username"></param>
/// <param name="EmailAddress"></param>
/// <param name="Password"></param>
public record RegisterRequest(string? Username, string? Email, string? Password);
public record RegisterResponse(string UserId, string DisplayId, string Username, string Email, bool IsAdmin, string Token, bool IsSuccess, string Error)
{
    public RegisterResponse() : this("", "", "", "", false, "", false, "") { }
}

public class RegisterHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public RegisterHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }

    public async Task<RegisterResponse> Handle(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = new RegisterResponse();
        
        var guid = Guid.NewGuid().ToString();
        var user = new ApplicationUser
        {
            Id = guid,
            UserName = request.Username,
            Email = request.Email,
            DisplayId = Slugify(request.Username, guid)
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, "User");

            if(roleResult.Succeeded)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var isAdmin = false;
                foreach (var role in userRoles)
                {
                    if (role.Equals("admin", StringComparison.InvariantCultureIgnoreCase))
                    {
                        isAdmin = true;
                        break;
                    }
                }
                var token = _tokenService.CreateToken(user, isAdmin);
                response = new RegisterResponse(user.Id, user.DisplayId, user.UserName, user.Email, isAdmin, token, true, "");
            }
            else
                response = new RegisterResponse("", "", "", "", false, "", false, "Failed to add user to role.");
        }
        else
            response = new RegisterResponse("", "", "", "", false, "", false, result.Errors.FirstOrDefault()?.Description ?? "Registration failed.");

        return response;

    }

    private static string Slugify(string username, string userId)
    {
        var normalized  = username?.ToLowerInvariant().Trim();
        var shortId = userId.Substring(0, 6);
        return $"{shortId}{normalized}";
    }
}

public static class RegisterEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/register", async (
            [FromBody] RegisterRequest request, 
            RegisterHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(request, ct); //need to add validation

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        });
    }
}