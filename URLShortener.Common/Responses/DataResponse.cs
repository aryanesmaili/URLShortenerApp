namespace URLShortener.Common.Responses;

public class DataResponse<T> : BaseResponse
{
    public T? Data { get; set; }
}
