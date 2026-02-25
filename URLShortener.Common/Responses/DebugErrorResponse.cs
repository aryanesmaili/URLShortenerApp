namespace SharedDataModels.Responses
{
    public class DebugErrorResponse
    {
        public string? Message { get; set; }
        public string? InnerException { get; set; }
        public string? StackTrace { get; set; }
    }
}