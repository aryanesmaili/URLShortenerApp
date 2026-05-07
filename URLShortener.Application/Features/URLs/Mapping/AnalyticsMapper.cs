using AutoMapper;
using URLShortener.Application.Features.URLs.DTOs;
using URLShortener.Domain.Entities.Analytics;

namespace URLShortener.Application.Features.URLs.Mapping;

public sealed class AnalyticsMapper : Profile
{
    public AnalyticsMapper()
    {
        CreateMap<URLAnalyticsModel, URLAnalyticsDTO>()
            .ReverseMap();
    }
}
