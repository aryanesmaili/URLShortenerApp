using SharedDataModels.Responses;
using URLShortener.Application.DTOs.EntityDTOs.URL;

namespace URLShortenerBlazor.Services.Interfaces
{
    public interface IShortenerService
    {
        Task<APIResponse<string>> DeleteURL(int urlID);
        string FakeShortener(string longURL);
        Task<APIResponse<List<URLShortenResponse>>> ShortenBatch(List<URLCreateDTO> createDTO);
        Task<APIResponse<URLShortenResponse>> ShortenSingle(URLCreateDTO createDTO);
        Task<APIResponse<string>> ToggleActivation(int urlID);
        Task<APIResponse<string>> ToggleMonetization(int urlID);
    }
}