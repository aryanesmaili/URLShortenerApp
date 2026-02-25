using URLShortener.Application.DTOs;

namespace URLShortener.Application.Interfaces.Services.URL
{
    public interface IURLService
    {
        Task<URLDTO> GetURL(int urlID);
        Task<URLShortenResponse> AddURL(URLCreateDTO url, string username);
        Task<List<URLShortenResponse>> AddBatchURL(List<URLCreateDTO> batchURL, string username);
        Task ToggleMonetization(int URLID, string username);
        Task ToggleActivation(int URLID, string reqUsername);
        Task DeleteURL(int URLID, string reqUsername);
    }
}
