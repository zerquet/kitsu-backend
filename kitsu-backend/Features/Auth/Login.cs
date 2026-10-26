using kitsu.Shared.Dtos;
using kitsu_backend.Entities;
using kitsu_backend.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace kitsu_backend.Features.Authentication;
/// <summary>
/// Request to log user into the application.
/// </summary>
/// <param name="Email"></param>
/// <param name="Password"></param>
public record LoginRequest(string Email, string Password);

public class LoginResponse
{
    public ApplicationUserResponse? User { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Token { get; set; }
}

public class ApplicationUserResponse: ProfileResponse
{
    public string? Email { get; set; }
    public bool IsAdmin { get; set; }
}

public class LoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public LoginHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }
    public async Task<LoginResponse> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = new LoginResponse();
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            response.ErrorMessage = "User not found";
            return response;
        }
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
        if (result.Succeeded)
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

            response.User = new ApplicationUserResponse
            {
                Id = user.Id,
                DisplayId = user.DisplayId,
                Username = user.UserName,
                Email = user.Email,
                Bio = user.Bio,
                Location = user.Location,
                Birthday = user.Birthday,
                Gender = user.Gender,
                ProfilePhotoId = user.ProfilePhotoId,
                CoverPhotoId = user.CoverPhotoId,
                IsAdmin = isAdmin
            };
            response.Token = token;
            response.Success = true;
        }
        else
        {
            response.ErrorMessage = "Invalid username or password";
        }

        return response;
    }
}

public static class LoginEndpoint
{
    public static void Map(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/auth/login", async (
            [FromBody] LoginRequest request, 
            LoginHandler handler,
            CancellationToken ct) =>
        {
            var response = await handler.Handle(request, ct);

            if (response.Success) return Results.Ok(response);
            return response.ErrorMessage switch
            {
                "User not found" => Results.BadRequest(),
                "Invalid username or password" => Results.Unauthorized(),
                _ => Results.BadRequest()
            };
        });
    }
}
