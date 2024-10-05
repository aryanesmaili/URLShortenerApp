namespace URLShortenerAPI.Utility.Exceptions
{
    internal class ErrorResponse
    {
        public string Message { get; set; }
        public string InnerException { get; set; }
        public string StackTrace { get; set; }
    }
}