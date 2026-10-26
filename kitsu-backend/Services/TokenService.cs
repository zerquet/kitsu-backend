using kitsu_backend.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace kitsu_backend.Services
{
    /*Service to create JWT tokens. Expires in 7 days.*/
    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, bool isAdmin);
    }
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;
        public TokenService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:SigningKey"]));
        }

        public string CreateToken(ApplicationUser user, bool isAdmin)
        {
            //claims to identify user and dictate what they can('t) do
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName),
                new Claim("profile_photo_id", user.ProfilePhotoId ?? ""),
                new Claim("cover_photo_id", user.CoverPhotoId ?? ""),
                new Claim("display_id", user.DisplayId)
            };

            if (isAdmin)
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));

            //type of desired encryption 
            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
