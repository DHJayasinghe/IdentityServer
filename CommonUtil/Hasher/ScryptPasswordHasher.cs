using Scrypt;
using System;

namespace CommonUtil.Hasher
{
    public sealed class ScryptPasswordHasher
    {
        public static readonly ScryptPasswordHasher Moderate = new ScryptPasswordHasher(new ScryptPasswordHasherOptions
        {
            IterationCount = 16384,
            BlockSize = 8,
            ThreadCount = 2
        });
        public static readonly ScryptPasswordHasher Sensitive = new ScryptPasswordHasher(new ScryptPasswordHasherOptions
        {
            IterationCount = 32768,
            BlockSize = 8,
            ThreadCount = 4
        });

        private readonly ScryptPasswordHasherOptions HasherOptions = new ScryptPasswordHasherOptions();

        // password hashing strength
        private ScryptPasswordHasher(ScryptPasswordHasherOptions options) => HasherOptions = options;

        public string HashPassword(string password)
        {
            if (password == null) throw new ArgumentNullException(nameof(password));

            var encoder = new ScryptEncoder(HasherOptions.IterationCount, HasherOptions.BlockSize, HasherOptions.ThreadCount);
            return encoder.Encode(password);

        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword == null) throw new ArgumentNullException(nameof(hashedPassword));
            if (providedPassword == null) throw new ArgumentNullException(nameof(providedPassword));

            var encoder = new ScryptEncoder();
            var isValid = encoder.Compare(providedPassword, hashedPassword);

            return isValid ? PasswordVerificationResult.Success : PasswordVerificationResult.Failed;
        }
    }
}
