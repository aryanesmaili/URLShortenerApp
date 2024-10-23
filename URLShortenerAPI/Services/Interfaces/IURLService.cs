using SharedDataModels.DTOs;

namespace URLShortenerAPI.Services.Interfaces
{
    public interface IURLService
    {
        public Task<URLDTO> GetURL(int urlID);
        public Task<URLShortenResponse> AddURL(URLCreateDTO url, string username);
        public Task<List<URLShortenResponse>> AddBatchURL(List<URLCreateDTO> batchURL, string username);
        public Task ToggleActivation(int URLID, string reqUsername);
        public Task<string> ShortURLGenerator(string LongURL);
        public Task DeleteURL(int URLID, string reqUsername);
    }
}
