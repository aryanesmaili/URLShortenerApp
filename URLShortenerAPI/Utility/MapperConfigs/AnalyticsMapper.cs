using AutoMapper;
using URLShortenerAPI.Data.Entities.Analytics;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    internal class AnalyticsMapper : Profile
    {
        public AnalyticsMapper()
        {
            CreateMap<URLAnalyticsModel, URLAnalyticsDTO>();
        }
    }
}
