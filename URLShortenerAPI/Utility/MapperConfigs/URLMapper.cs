using AutoMapper;
using URLShortenerAPI.Data.Entites.URL;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    internal class URLMapper : Profile
    {
        public URLMapper()
        {
            CreateMap<URLModel, URLDTO>();
        }
    }
}
