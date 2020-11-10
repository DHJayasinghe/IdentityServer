namespace Identity.API.Models
{
    public sealed class ExchangeRefreshTokenModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
