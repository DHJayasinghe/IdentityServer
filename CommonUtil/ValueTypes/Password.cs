using CommonUtil.Hasher;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommonUtil.ValueTypes
{
    public sealed class Password : ValueObject
    {
        public string HashValue { get; set; }

        private Password(string hashValue) => HashValue = hashValue;

        public static Result<Password> Create(Maybe<string> textPasswordOrNothing, bool enforceStrongPassword = true)
        {
            Result<string> passwordResult = textPasswordOrNothing
               .ToResult("Password should not be empty")
               .OnSuccess(password => password.Trim())
               .Ensure(password => password.Length >= 8, "Password is too short")
               .Ensure(password => !enforceStrongPassword || Regex.IsMatch(password, @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?!.*\s).*$"), "Strong password is required")
               .Ensure(password => password != string.Empty, "Password should not be empty");

            if (passwordResult.IsFailure)
                return Result.Fail<Password>(passwordResult.Error);

            string hashedPassword = ScryptPasswordHasher.Sensitive.HashPassword(textPasswordOrNothing.Unwrap());
            return Result.Ok(new Password(hashedPassword));
        }

        public static Result<Password> Verify(Maybe<string> hashedPasswordOrNothing, Maybe<string> textPasswordOrNothing)
        {
            Result<string> textPasswordResult = textPasswordOrNothing
               .ToResult("Password should not be empty")
               .OnSuccess(password => password.Trim())
               .Ensure(password => password != string.Empty, "Password should not be empty");

            Result<string> hashedPasswordResult = hashedPasswordOrNothing
               .ToResult("Hashed password should not be empty")
               .OnSuccess(password => password.Trim())
               .Ensure(password => password != string.Empty, "Hashed password should not be empty");

            Result result = Result.Combine(textPasswordResult, hashedPasswordResult);
            if (result.IsFailure)
                return Result.Fail<Password>(result.Error);

            var passwordResult = ScryptPasswordHasher.Sensitive.VerifyHashedPassword(hashedPasswordOrNothing.Unwrap(), textPasswordOrNothing.Unwrap());
            if (passwordResult == PasswordVerificationResult.Failed)
                return Result.Fail<Password>("Invalid account credentials provided");

            // just return OK response with original hashed password from DB
            return Result.Ok(new Password(hashedPasswordOrNothing.Unwrap()));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HashValue;
        }

        public static explicit operator Password(string password) => Create(password).Value;
        public static implicit operator string(Password password) => password.HashValue;
    }
}
