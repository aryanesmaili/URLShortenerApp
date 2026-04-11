using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.Category;
using URLShortener.Domain.Entities.URLCategory;

namespace URLShortenerAPI.Responses.MapperConfigs;

public sealed class URLCategoryMapper : Profile
{
    public URLCategoryMapper()
    {
        CreateMap<URLCategoryModel, CategoryDTO>()
            .ReverseMap();

        CreateMap<URLCategoryModel, CategorySummaryDTO>();
    }
}
