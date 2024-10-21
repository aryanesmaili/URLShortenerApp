namespace SharedDataModels.Responses
{
    public class APIResponse<T> where T : class
    {
        public ErrorType? ErrorType { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public T? Result { get; set; }
    }
    public enum ErrorType
    {
        NotFound,
        ArgumentException,
        ArgumentNullException,
        ValidationException,
        NotAuthorizedException
    }
}