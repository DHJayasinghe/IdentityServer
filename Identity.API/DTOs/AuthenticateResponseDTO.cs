using Identity.API.Entities;

namespace Identity.API
{
    public sealed class AuthenticateResponseDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        public AccessTokenDTO AccessToken { get; }
        public string RefreshToken { get; }

        public AuthenticateResponseDTO(AccessTokenDTO accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public AuthenticateResponseDTO(AppUser user, AccessTokenDTO accessToken, string refreshToken)
            : this(accessToken, refreshToken)
        {

            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
        }
    }
}
