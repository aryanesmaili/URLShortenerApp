using URLShortenerAPI.Data.Entites.URL;

namespace URLShortenerAPI.Services.Interfaces
{
    internal interface IURLService
    {
        public Task<URLDTO> GetURL(int urlID);
        public Task<URLDTO> AddURL(URLCreateDTO url);
        public Task ToggleActivation(int URLID, string reqUsername);
        public string ShortURLGenerator(string LongURL);
        public URLDTO URLModelToDTO(URLModel url);
    }
}
