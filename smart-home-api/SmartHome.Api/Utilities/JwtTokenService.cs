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
        private readonly byte[] _signingKey;

        public JwtTokenService(IConfiguration configuration, UserManager<HomeUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
            var jwtKey = configuration["Jwt:Key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException("Jwt:Key is required and must be at least 32 bytes.");
            }

            _signingKey = Encoding.UTF8.GetBytes(jwtKey);
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

            var key = new SymmetricSecurityKey(_signingKey);
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
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(_signingKey),
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
