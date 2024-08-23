﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace APICatalogo.Services
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateAcessToken(IEnumerable<Claim> claims, IConfiguration _config);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToKen(string token, IConfiguration _config);
    }
}