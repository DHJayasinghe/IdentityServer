using System;
using System.Security.Cryptography;

namespace Identity.API.Common
{
    public interface ITokenFactory
    {
        string GenerateToken(int size = 32);
    }

    internal sealed class TokenFactory : ITokenFactory
    {
        public string GenerateToken(int size = 32)
        {
            var randomNumber = new byte[size];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
