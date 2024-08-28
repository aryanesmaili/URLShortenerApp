using AutoMapper;
using URLShortenerAPI.Data.Entites.URL;
using URLShortenerAPI.Data.Entites.URLCategory;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    internal class URLCategoryMapper : Profile
    {
        public URLCategoryMapper()
        {
            CreateMap<URLCategoryModel, CategoryDTO>()
                .ForMember(x => x.URLs, opt => opt.MapFrom<CategoryURLsResolver>());
        }
    }

    internal class CategoryURLsResolver(IMapper mapper) : IValueResolver<URLCategoryModel, CategoryDTO, List<URLDTO>?>
    {
        private readonly IMapper _mapper = mapper;

        public List<URLDTO>? Resolve(URLCategoryModel source, CategoryDTO destination, List<URLDTO>? destMember, ResolutionContext context)
        {
            return source.URLs?.Select(x => _mapper.Map<URLDTO>(x)).ToList();
        }
    }
}
