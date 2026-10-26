using kitsu_backend.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using kitsu_backend.Database;

namespace kitsu_backend.Extensions
{
    public static class IdentityServiceExtensions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
        {
            //TO DO: add db context config here since it's IdentityDb?
            services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true; //added
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
            //To add roles, make sure to add it before AddEntity... Ref: https://stackoverflow.com/a/75307288/20829897

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                options.DefaultChallengeScheme =
                options.DefaultForbidScheme =
                options.DefaultScheme =
                options.DefaultSignInScheme =
                options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = config["JWT:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = config["JWT:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JWT:SigningKey"])
                    )
                };
            });

            //Need to add these if using AddIdentityCore().
            //Ref: https://stackoverflow.com/a/56551234/20829897, https://stackoverflow.com/a/55361961/20829897
            // Hosting doesn't add IHttpContextAccessor by default
            services.AddHttpContextAccessor();
            // Identity services
            services.TryAddScoped<IUserValidator<ApplicationUser>, UserValidator<ApplicationUser>>();
            services.TryAddScoped<IPasswordValidator<ApplicationUser>, PasswordValidator<ApplicationUser>>();
            services.TryAddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
            services.TryAddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.TryAddScoped<IRoleValidator<IdentityRole>, RoleValidator<IdentityRole>>();
            // No interface for the error describer so we can add errors without rev'ing the interface
            services.TryAddScoped<IdentityErrorDescriber>();
            services.TryAddScoped<ISecurityStampValidator, SecurityStampValidator<ApplicationUser>>();
            services.TryAddScoped<ITwoFactorSecurityStampValidator, TwoFactorSecurityStampValidator<ApplicationUser>>();
            services.TryAddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, UserClaimsPrincipalFactory<ApplicationUser>>();
            services.TryAddScoped<UserManager<ApplicationUser>>();
            services.TryAddScoped<SignInManager<ApplicationUser>>();
            services.TryAddScoped<RoleManager<IdentityRole>>();

            return services;
        }
    }
}
