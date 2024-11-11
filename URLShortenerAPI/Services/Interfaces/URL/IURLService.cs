using SharedDataModels.DTOs;

namespace URLShortenerAPI.Services.Interfaces.URLRelated
{
    public interface IURLService
    {
        public Task<URLDTO> GetURL(int urlID);
        public Task<URLShortenResponse> AddURL(URLCreateDTO url, string username);
        public Task<List<URLShortenResponse>> AddBatchURL(List<URLCreateDTO> batchURL, string username);
        public Task ToggleMonetization(int URLID, string username);
        public Task ToggleActivation(int URLID, string reqUsername);
        public Task DeleteURL(int URLID, string reqUsername);
    }
}
