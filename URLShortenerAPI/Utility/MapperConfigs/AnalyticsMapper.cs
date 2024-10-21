using AutoMapper;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.Analytics;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    internal class AnalyticsMapper : Profile
    {
        public AnalyticsMapper()
        {
            CreateMap<URLAnalyticsModel, URLAnalyticsDTO>();
        }
    }
}
