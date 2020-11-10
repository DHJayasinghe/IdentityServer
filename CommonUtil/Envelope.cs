using System;

namespace CommonUtil
{
    public class Envelope<T>
    {
        public T Result { get; }
        public string ErrorMessage { get; }
        public DateTime TimeGenerated { get; }
        public bool Success { get; }

        protected internal Envelope(T result, string errorMessage, bool success = false)
        {
            Result = result;
            ErrorMessage = errorMessage;
            TimeGenerated = DateTime.UtcNow;
            Success = success;
        }
    }

    public class Envelope : Envelope<string>
    {
        protected Envelope(string errorMessage, bool success = false)
            : base(string.Empty, errorMessage, success) { }

        public static Envelope<T> Ok<T>(T result) =>
            new Envelope<T>(result, string.Empty, success: true);

        public static Envelope Ok() =>
            new Envelope(string.Empty, success: true);

        public static Envelope Error(string errorMessage) =>
            new Envelope(errorMessage);

        public static Envelope<T> Error<T>(string errorMessage) =>
            new Envelope<T>(default, errorMessage);
    }

    public class EnvelopeResponse<T>
    {
        public T Result { set; get; }
        public string ErrorMessage { set; get; }
        public DateTime TimeGenerated { set; get; }
        public bool Success { set; get; }

        public static EnvelopeResponse<string> Ok() =>
           new EnvelopeResponse<string>()
           {
               Success = true
           };
    }
}
