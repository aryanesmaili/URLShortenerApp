using AutoMapper;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.URLCategory;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    internal class URLCategoryMapper : Profile
    {
        public URLCategoryMapper()
        {
            CreateMap<URLCategoryModel, CategoryDTO>()
                .ForMember(x => x.URLs, opt => opt.Ignore());
        }
    }

    internal class CategoryURLsResolver(IMapper mapper) : IValueResolver<URLCategoryModel, CategoryDTO, List<URLDTO>?>
    {
        private readonly IMapper _mapper = mapper;

        public List<URLDTO>? Resolve(URLCategoryModel source, CategoryDTO destination, List<URLDTO>? destMember, ResolutionContext context)
        {
            return source.URLs?.Select(_mapper.Map<URLDTO>).ToList();
        }
    }
}
