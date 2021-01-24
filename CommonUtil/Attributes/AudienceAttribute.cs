using System;

namespace CommonUtil.Attributes
{
    public sealed class AudienceAttribute : Attribute
    {
        public string Audience { get; }
        public AudienceAttribute(string audience)
        {
            Audience = audience;
        }
    }
}
