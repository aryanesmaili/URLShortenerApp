using SharedDataModels.DTOs;
using SharedDataModels.Responses;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IShortenerService
    {
        public Task<APIResponse<string>> DeleteURL(int urlID);
        public string FakeShortener(string longURL);
        public Task<APIResponse<List<URLShortenResponse>>> ShortenBatch(List<URLCreateDTO> createDTO);
        public Task<APIResponse<URLShortenResponse>> ShortenSingle(URLCreateDTO createDTO);
        Task<APIResponse<string>> ToggleActivation(int urlID);
    }
}