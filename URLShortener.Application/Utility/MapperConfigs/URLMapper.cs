using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Domain.Entities.URL;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    public class URLMapper : Profile
    {
        public URLMapper()
        {
            CreateMap<URLModel, URLDTO>()
                .ReverseMap();

            CreateMap<URLCreateDTO, URLModel>();

        }
    }
}
