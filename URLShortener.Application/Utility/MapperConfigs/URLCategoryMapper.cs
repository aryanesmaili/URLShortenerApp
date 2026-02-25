using AutoMapper;
using URLShortener.Application.DTOs;
using URLShortener.Domain.Entities.URLCategory;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    public class URLCategoryMapper : Profile
    {
        public URLCategoryMapper()
        {
            CreateMap<URLCategoryModel, CategoryDTO>()
                .ForMember(x => x.URLs, opt => opt.Ignore());
        }
    }
}
