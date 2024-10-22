namespace SharedDataModels.Responses
{
    public class APIResponse<T> where T : class
    {
        public bool Success { get; set; } = false;
        public ErrorType? ErrorType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();
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