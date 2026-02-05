using Domain.Authentication;
using Domain.Environment;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Business.Authentication
{
    public class AuthenticationService(IOptions<TokenSetting> tokenSetting) : IAuthenticationService
    {
        private readonly TokenSetting _tokenSetting = tokenSetting.Value;

        public string GenerateUserToken(Guid userId, short userRoleId)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSetting.UserScheme.Key));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, userRoleId.ToString()),
            };

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _tokenSetting.Issuer,
                audience: _tokenSetting.UserScheme.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_tokenSetting.UserScheme.ExpiresInMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public TokenUserInfo? GetTokenUserInfo(string token)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenSetting.UserScheme.Key));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _tokenSetting.Issuer,
                ValidAudience = _tokenSetting.UserScheme.Audience,
                IssuerSigningKey = key,
                ValidateLifetime = true
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

                var userId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                var userRoleId = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (userId == null || userRoleId == null)
                {
                    return null;
                }

                return new()
                {
                    UserId = Guid.Parse(userId),
                    UserRoleId = short.Parse(userRoleId)
                };
            }
            catch
            {
                return null;
            }
        }
    }
}
