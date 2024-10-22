using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IShortenerService
    {
        public Task<APIResponse<string>> DeleteURL(int urlID);
        public string FakeShortener(string longURL);
        public Task<APIResponse<List<BatchURLResponse>>> ShortenBatch(List<URLCreateDTO> createDTO);
        public Task<APIResponse<URLDTO>> ShortenSingle(URLCreateDTO createDTO);
    }
}