using AutoMapper;
using URLShortener.Application.DTOs;
using URLShortener.Domain.Entities.ClickInfo;
using URLShortener.Domain.Entities.URL;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    public class URLMapper : Profile
    {
        public URLMapper()
        {
            CreateMap<URLModel, URLDTO>()
                .ForMember(x => x.Categories, opt => opt.MapFrom<URLCategoryResolver>())
                ;

            CreateMap<URLCreateDTO, URLModel>()
                .ForMember(u => u.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(u => u.Clicks, opt => opt.MapFrom(src => new List<ClickInfoModel>()))
                .ForMember(u => u.Categories, opt => opt.Ignore())
                ;

        }
    }

    public class URLCategoryResolver : IValueResolver<URLModel, URLDTO, List<CategoryDTO>?>
    {
        private readonly IMapper _mapper;

        public URLCategoryResolver(IMapper mapper)
        {
            _mapper = mapper;
        }

        public List<CategoryDTO>? Resolve(URLModel source, URLDTO destination, List<CategoryDTO>? destMember, ResolutionContext context)
        {
            return source.Categories?.Select(x => _mapper.Map<CategoryDTO>(x)).ToList();
        }
    }
}
