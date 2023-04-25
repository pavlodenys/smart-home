using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SmartHome.Data.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartHome.Api.Utilities
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<HomeUser> _userManager;

        public JwtTokenService(IConfiguration configuration, UserManager<HomeUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public string GenerateAccessToken(HomeUser user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
        };

            //var roles = await _userManager.GetRolesAsync(user);
            //claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "jwt_key"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                //expires: DateTime.UtcNow.AddMinutes(15),
                expires: DateTime.UtcNow.AddHours(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "jwt_key");
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero
                }, out var validatedToken);

                return new ClaimsPrincipal();//todo: update
            }
            catch
            {
                return null;
            }
        }
    }

    public interface IJwtTokenService
    {
        string GenerateAccessToken(HomeUser user);
        ClaimsPrincipal? GetPrincipalFromToken(string token);
    }
}
