using URLShortenerAPI.Data.Entities.URL;

namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IURLService
    {
        public Task<URLDTO> GetURL(int urlID);
        public Task<URLDTO> AddURL(URLCreateDTO url, string username);
        public Task ToggleActivation(int URLID, string reqUsername);
        public Task<string> ShortURLGenerator(string LongURL);
        public URLDTO URLModelToDTO(URLModel url);
    }
}
