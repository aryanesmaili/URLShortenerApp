namespace SharedDataModels.Responses;

/// <summary>
/// A standardized response model for API endpoints, encapsulating success status, error information, and result data. This structure allows for consistent handling of API responses across the application, making it easier to manage success and failure cases in a unified way.
/// </summary>
/// <typeparam name="T"></typeparam>
public class APIResponse<T>
{
    /// <summary>
    /// <c>True</c> if the API operation was successful, <c>False</c> if it failed. This field is the primary indicator of the outcome of the API call and should be checked before processing any result data or error information.
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// <c>Null</c> on Success, Indicates the type of error that occurred, if any.
    /// </summary>
    public ErrorType? ErrorType { get; set; }
    /// <summary>
    /// <c>Null</c> on Success, <c>Error message</c> on Failure.
    /// </summary>
    public string? Message { get; set; }
    /// <summary>
    /// For Validation errors, this will contain <c>a list of error messages</c>. For other error types, this may be <c>Null</c> or contain additional error details.
    /// </summary>
    public List<string>? Errors { get; set; }
    /// <summary>
    /// <c>Null</c> on Failure, <c>Result</c> on Success.
    /// </summary>
    public T? Result { get; set; }
}
/// <summary>
/// A non-generic version of APIResponse for cases where no specific result data is needed, such as simple success/failure responses or when only error information is relevant.
/// </summary>
public class APIResponse : APIResponse<object> { }


public enum ErrorType
{
    NotFound,
    Argument,
    Validation,
    Unauthorized,
    CaptchaFailure,
    InsufficientBalance,
    PaymentFailed,
    InternalError,
}