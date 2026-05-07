using AutoMapper;
using URLShortener.Application.Features.Categories.DTOs;
using URLShortener.Domain.Entities.URLCategory;

namespace URLShortener.Application.Features.Categories.Mapping;

public sealed class URLCategoryMapper : Profile
{
    public URLCategoryMapper()
    {
        CreateMap<URLCategoryModel, CategoryDTO>()
            .ReverseMap();

        CreateMap<URLCategoryModel, CategorySummaryDTO>();
    }
}
