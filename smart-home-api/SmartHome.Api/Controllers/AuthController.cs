using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Api.Utilities;
using SmartHome.Data.DTO;
using SmartHome.Data.Entities;
using SmartHome.Logic;

namespace SmartHome.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<HomeUser> _userManager;
        private readonly SignInManager<HomeUser> _signInManager;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRepository<RefreshToken, RefreshTokenDto> _tokenRepository;

        public AuthController(UserManager<HomeUser> userManager,
            SignInManager<HomeUser> signInManager,
            IJwtTokenService jwtTokenService,
            IRepository<RefreshToken, RefreshTokenDto> tokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _tokenRepository = tokenRepository;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            if(model.Password != model.ConfirmPassword)
            {
                return BadRequest(new {message = "passwords didn't match"});
            }

            var user = new HomeUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = result.Errors });
            }

            //await _userManager.AddToRoleAsync(user, "User");

            var token = _jwtTokenService.GenerateAccessToken(user);

            return Ok(new
            {
                token,
                refresh_token = GenerateRefreshToken(user)
            });
        }


        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto model)
        {
            //var user = await _userManager.FindByEmailAsync(model.Email);
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid credentials" });
            }

            var token = _jwtTokenService.GenerateAccessToken(user);

            return Ok(new
            {
                token,
                refresh_token = GenerateRefreshToken(user)
            });
        }

        private string GenerateRefreshToken(HomeUser user)
        {
            // Generate a refresh token and save it to the database
            var refreshToken = new RefreshTokenDto
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };
            //_tokenRepository.RefreshTokens.Add(refreshToken);
            //_tokenRepository.SaveChanges();

            _tokenRepository.Create(refreshToken);

            return refreshToken.Token;
        }

        [HttpPost("refresh-token")]
        [Authorize]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto model)
        {
            var principal = _jwtTokenService.GetPrincipalFromToken(model.Token);
            var username = principal?.Identity?.Name;
            if(username == null) {
                return Unauthorized(new { message = "User not found" });
            }
            var user = await _userManager.FindByNameAsync(username);

            if (user == null || !user.IsActive || user.RefreshTokens.All(rt => rt.Token != model.Token))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var existingRefreshToken = user.RefreshTokens.Single(rt => rt.Token == model.Token);

            if (existingRefreshToken.IsExpired)
            {
                //_context.RefreshTokens.Remove(existingRefreshToken);
                //_context.SaveChanges();

                await _tokenRepository.Delete(existingRefreshToken);
                return Unauthorized(new { message = "Expired token" });
            }

            var newToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            await _tokenRepository.Delete(existingRefreshToken);
            await _tokenRepository.Create(new RefreshTokenDto
            {
                CreatedAt = DateTime.UtcNow,
                Token = newToken,
                UserId = user.Id
            });

            return Ok(new
            {
                token = newToken,
                refresh_token = newRefreshToken
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("protected")]
        public IActionResult Protected()
        {
            return Ok(new { message = "Protected resource" });
        }
    }

}
