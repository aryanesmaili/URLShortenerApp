using SharedDataModels.DTOs;

namespace URLShortenerAPI.Services.Interfaces
{
    public interface IURLService
    {
        public Task<URLDTO> GetURL(int urlID);
        public Task<URLDTO> AddURL(URLCreateDTO url, string username);
        public Task<List<BatchURLResponse>> AddBatchURL(List<URLCreateDTO> batchURL, string username);
        public Task ToggleActivation(int URLID, string reqUsername);
        public Task<string> ShortURLGenerator(string LongURL);
        public Task DeleteURL(int URLID, string reqUsername);
    }
}
