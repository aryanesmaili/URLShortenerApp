using AutoMapper;
using URLShortener.Application.Features.URLs.DTOs;
using URLShortener.Domain.Entities.URL;

namespace URLShortener.Application.Features.URLs.Mapping;

public sealed class URLMapper : Profile
{
    public URLMapper()
    {
        CreateMap<URLModel, URLDTO>()
            .ReverseMap();

        CreateMap<URLCreateDTO, URLModel>();

    }
}
