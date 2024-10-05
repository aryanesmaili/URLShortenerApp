namespace URLShortenerAPI.Utility.Exceptions
{
    internal class ErrorResponse
    {
        public required string Message { get; set; }
        public required string InnerException { get; set; }
        public required string StackTrace { get; set; }
    }
}