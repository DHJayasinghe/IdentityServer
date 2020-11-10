namespace Identity.API
{
    public sealed class ExchangeRefreshTokenResponseDTO
    {
        public AccessTokenDTO AccessToken { get; }
        public string RefreshToken { get; }

        public ExchangeRefreshTokenResponseDTO(AccessTokenDTO accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
