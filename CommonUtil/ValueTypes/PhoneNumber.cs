using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CommonUtil.ValueTypes
{
    public sealed class PhoneNumber : ValueObject
    {
        public string Value { get; }

        private PhoneNumber(string contactNo) => Value = contactNo;

        public static Result<PhoneNumber> Create(Maybe<string> phoneNoOrNothing)
        {
            return phoneNoOrNothing.ToResult("Phone number should not be empty")
                .OnSuccess(phoneNo => phoneNo.Trim())
                .Ensure(phoneNo => phoneNo != string.Empty, "Phone number should not be empty.")
                .Ensure(phoneNo => phoneNo.Length <= 15, "Phone number is too long.")
                .Ensure(phoneNo => Regex.IsMatch(phoneNo, @"\+[0-9]+$"), "Phone number is invalid")//should start with + follows digits
                .Map(phoneNo => new PhoneNumber(phoneNo));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public static explicit operator PhoneNumber(string phoneNumber) => Create(phoneNumber).Value;
        public static implicit operator string(PhoneNumber phoneNumber) => phoneNumber.Value;
    }
}
