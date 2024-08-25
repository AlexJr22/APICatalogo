using APICatalogo.DTOs;
using APICatalogo.Models;
using APICatalogo.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalogo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService tokenService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration configuration;

        public AuthController(
            ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            this.tokenService = tokenService;
            this.userManager = userManager;
            this.roleManager = roleManager;
            this.configuration = configuration;
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login([FromBody] LoginModelDTO loginModel)
        {
            var user = await userManager.FindByNameAsync(loginModel.UserName!);

            if (user is not null && await userManager.CheckPasswordAsync(user, loginModel.Password!))
            {
                var userRoles = await userManager.GetRolesAsync(user);

                var authClaim = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(ClaimTypes.Email, user.Email!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                foreach (var userRole in userRoles)
                {
                    authClaim.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = tokenService.GenerateAcessToken(authClaim, configuration);

                var refreshToken = tokenService.GenerateRefreshToken();

                _ = int.TryParse(configuration["JWT:RefreshTokenValidityInMinutes"], out int refreshTokenValidityInMinutes);

                user.RefreshTokenExpireTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);

                await userManager.UpdateAsync(user);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
        {
            var userExists = await userManager.FindByNameAsync(model.UserName!);

            if (userExists is not null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new Response { Status = "Erro", Messege = "user already exists!" });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.UserName,
            };

            var result = await userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new Response { Messege = "User creation Failed!", Status = "500" });
            }

            return Ok(new Response { Status = "Seccess", Messege = "User created successfully!" });
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(TokenModelDTO tokenModel)
        {
            if (tokenModel is null)
            {
                return BadRequest("Invalid client request");
            }

            string? accessToken = tokenModel.AcessToken
                ?? throw new ArgumentNullException(nameof(tokenModel));

            string? refreshToken = tokenModel.RefreshToken
                ?? throw new ArgumentNullException(nameof(tokenModel));

            var principal = tokenService.GetPrincipalFromExpiredToKen(accessToken!, configuration);

            if (principal is null)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            string username = principal.Identity.Name;

            var user = await userManager.FindByNameAsync(username!);

            if (user is null
                || user.RefreshToken != refreshToken
                || user.RefreshTokenExpireTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            var newAccessToken = tokenService.GenerateAcessToken(
                principal.Claims.ToList(), configuration);

            var newRefreshToken = tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken,
            });
        }
    }
}
