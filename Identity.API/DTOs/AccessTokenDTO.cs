namespace Identity.API
{
    public sealed class AccessTokenDTO
    {
        public string Token { get; }
        public int ExpiresIn { get; }

        public AccessTokenDTO(string token, int expiresIn)
        {
            Token = token;
            ExpiresIn = expiresIn;
        }
    }
}
