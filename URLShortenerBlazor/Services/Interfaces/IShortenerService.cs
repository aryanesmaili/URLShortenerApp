using SharedDataModels.DTOs;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IShortenerService
    {
        string FakeShortener(string longURL);
        Task<List<BatchURLAdditionResponse>> ShortenBatch(List<URLCreateDTO> createDTO);
        Task<URLDTO> ShortenSingle(URLCreateDTO createDTO);
    }
}