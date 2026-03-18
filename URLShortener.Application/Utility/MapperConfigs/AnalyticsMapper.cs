using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.URL;
using URLShortener.Domain.Entities.Analytics;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    public class AnalyticsMapper : Profile
    {
        public AnalyticsMapper()
        {
            CreateMap<URLAnalyticsModel, URLAnalyticsDTO>()
                .ReverseMap();
        }
    }
}
