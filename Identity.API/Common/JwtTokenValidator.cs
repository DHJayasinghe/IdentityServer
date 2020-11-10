using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Identity.API.Common
{
    public interface IJwtTokenValidator
    {
        ClaimsPrincipal GetPrincipalFromToken(string token, string secretKey);
    }

    internal sealed class JwtTokenValidator : IJwtTokenValidator
    {
        private readonly IJwtTokenHandler _jwtTokenHandler;

        public JwtTokenValidator(IJwtTokenHandler jwtTokenHandler) => _jwtTokenHandler = jwtTokenHandler;

        public ClaimsPrincipal GetPrincipalFromToken(string token, string secretKey)
        {
            return _jwtTokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false // we check expired tokens here
            });
        }
    }
}
