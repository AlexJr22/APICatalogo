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
    }
}
