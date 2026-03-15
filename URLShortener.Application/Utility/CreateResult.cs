using URLShortener.Common.Responses;

namespace URLShortener.Application.Utility;

public static class CreateResult
{
    // Functions that make Success Results.
    public static BaseResponse CreateSuccess()
        => new() { Success = true, Message = ResultMessages.LiteralMessages[OperationOutcomes.Success] };

    public static BaseResponse CreateSuccess(OperationOutcomesFormatted formatted, params object[] args)
        => new() { Success = true, Message = string.Format(ResultMessages.FormattedMessages[formatted], args) };

    public static DataResponse<T> CreateDataSuccess<T>(T data)
        => new() { Success = true, Data = data, Message = ResultMessages.LiteralMessages[OperationOutcomes.Success] };

    public static DataResponse<T> CreateDataSuccess<T>(T data, OperationOutcomesFormatted formatted, params object[] args)
        => new() { Success = true, Data = data, Message = string.Format(ResultMessages.FormattedMessages[formatted], args) };

    public static PagedResult<T> CreatePagedSuccess<T>(ICollection<T> data, int pageNumber, int pageSize, int totalCount, int totalPages)
        => new() { Success = true, Message = ResultMessages.LiteralMessages[OperationOutcomes.Success], Items = [.. data], PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount, TotalPages = totalPages };

    public static PagedResult<T> CreatePagedSuccess<T>(ICollection<T> data, int pageNumber, int pageSize, int totalCount, int totalPages, OperationOutcomes outcome)
        => new() { Success = true, Message = ResultMessages.LiteralMessages[outcome], Items = [.. data], PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount, TotalPages = totalPages };

    public static PagedResult<T> CreatePagedSuccess<T>(ICollection<T> data, int pageNumber, int pageSize, int totalCount, int totalPages, OperationOutcomesFormatted outcome, params object[] args)
        => new() { Success = true, Message = string.Format(ResultMessages.FormattedMessages[outcome], args), Items = [.. data], PageNumber = pageNumber, PageSize = pageSize, TotalCount = totalCount, TotalPages = totalPages };

    // Functions that make Failure Results.
    public static BaseResponse CreateFailure(string? message = null)
        => new() { Success = false, Message = message ?? ResultMessages.LiteralMessages[OperationOutcomes.InternalError] };

    public static BaseResponse CreateFailure(OperationOutcomesFormatted reason, params object[] args)
        => new() { Success = false, Message = string.Format(ResultMessages.FormattedMessages[reason], args) };

    public static PagedResult<T> CreateFailure<T>(OperationOutcomes reason)
        => new() { Success = false, Message = ResultMessages.LiteralMessages[reason] };

    public static PagedResult<T> CreateFailure<T>(OperationOutcomesFormatted reason, params object[] args)
        => new() { Success = false, Message = string.Format(ResultMessages.FormattedMessages[reason], args) };

}
