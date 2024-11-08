namespace SharedDataModels.Responses
{
    public class APIResponse<T> where T : class
    {
        public bool Success { get; set; } = false;
        public ErrorType? ErrorType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = [];
        public T? Result { get; set; }
    }

    public class CaptchaVerificationResponse
    {
        public bool Success { get; set; }
        public string? Challenge_ts { get; set; } // Time the challenge was solved (ISO timestamp)
        public string? Hostname { get; set; } // Hostname of the served challenge
        public List<string>? ErrorCodes { get; set; } // List of error codes, if any
        public string? Action { get; set; } // Action identifier for client validation
        public string? Cdata { get; set; } // Additional customer data if passed
    }

    public enum ErrorType
    {
        NotFound,
        ArgumentException,
        ArgumentNullException,
        ValidationException,
        NotAuthorizedException,
        CaptchaFailureException
    }
}