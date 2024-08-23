using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace APICatalogo.Services
{
    public class TokenService : ITokenService
    {
        public JwtSecurityToken GenerateAcessToken(IEnumerable<Claim> claims, IConfiguration _config)
        {
            var key = _config.GetSection("JWT").GetValue<string>("SecretKey")
                ?? throw new InvalidOperationException("A chave secreta é nula!");

            var privateKey = Encoding.UTF8.GetBytes(key);

            var singningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(privateKey), SecurityAlgorithms.HmacSha256Signature);

            var tokenCredentials = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_config.GetSection("JWT").GetValue<double>("TokenValidityInMinutes")),
                Audience = _config.GetSection("JWT").GetValue<string>("ValidAudiance"),
                Issuer = _config.GetSection("JWT").GetValue<string>("ValidIssuer"),
                SigningCredentials = singningCredentials,
            };
                
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateJwtSecurityToken(tokenCredentials);

            return token;
        }

        public string GenerateRefreshToken()
        {
            var secureRandomBytess = new byte[128];

            using var randomNumberGenerator = RandomNumberGenerator.Create();
            RandomNumberGenerator.GetBytes(secureRandomBytess);

            var refreshToken = Convert.ToBase64String(secureRandomBytess);

            return refreshToken;
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToKen(string token, IConfiguration _config)
        {
            throw new NotImplementedException();
        }
    }
}
