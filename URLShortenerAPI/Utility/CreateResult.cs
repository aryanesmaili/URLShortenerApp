using SharedDataModels.Responses;
using URLShortener.Common.Responses;

namespace URLShortenerAPI.Utility;

public static class CreateResult
{
    // Success Results

    /// <summary>
    /// A simple success response without any data. For Operations that don't return a result but you want to indicate success. This Method Lets you Indicate the success of the operation without providing any additional data.
    /// </summary>
    /// <returns>A success response without any data.</returns>
    public static APIResponse Success() => new() { Success = true };
    /// <summary>
    /// A success response that includes data. For Operations that return a result. This Method Lets you Indicate both the success of the operation and provide the resulting data.
    /// </summary>
    /// <typeparam name="T">The type of the data being returned.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <returns>A success response containing the specified data.</returns>
    public static APIResponse<T> Success<T>(T data) => new() { Success = true, Result = data };

    // Failure Results

    /// <summary>
    /// A failure response that includes an error type and an optional error message. For Operations that fail and you want to provide information about the failure.
    /// </summary>
    /// <param name="errorType">The type of error that occurred.</param>
    /// <param name="errorMessage">An optional message providing additional details about the error.</param>
    /// <returns>A failure response containing the specified error information.</returns>
    public static APIResponse Failure(ErrorType errorType, string? errorMessage)
        => new() { Success = false, Message = errorMessage, ErrorType = errorType };

    /// <summary>
    /// A failure response that includes a list of validation error messages. For operations that fail due to validation errors.
    /// </summary>
    /// <param name="errorMessages">The list of validation error messages.</param>
    /// <returns>A failure response containing the specified validation errors.</returns>
    public static APIResponse Failure(List<string> errorMessages)
        => new() { Success = false, Errors = errorMessages, ErrorType = ErrorType.Validation };

    /// <summary>
    /// A paged success response that includes a list of data items along with pagination information. For operations that return a paginated list of results. This Method Lets you Indicate the success of the operation, provide the resulting data, and include details about the pagination such as the current page number, page size, total item count, and total number of pages.
    /// </summary>
    /// <typeparam name="T">The type of the data items being returned.</typeparam>
    /// <param name="data">The list of data items.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalItems">The total number of items.</param>
    /// <returns>A paged success response containing the specified data and pagination information.</returns>
    public static PagedResponse<T> Paged<T>(PagedResult<T> paged)
        => new()
        {
            Success = true,
            Result = paged.Items,
            TotalCount = paged.TotalCount,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
            TotalPages = paged.TotalPages
        };
}
