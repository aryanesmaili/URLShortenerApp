using AutoMapper;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.URLCategory;
using URLShortenerAPI.Data.Entities.User;

namespace URLShortenerAPI.Responses.MapperConfigs
{
    internal class UserMapper : Profile
    {
        public UserMapper()
        {
            CreateMap<UserModel, UserDTO>()
                .ForMember(x => x.URLs, opt => opt.MapFrom<UserURLsResolver>())
                .ForMember(x => x.Categories, opt => opt.MapFrom<UserCategoriesResolver>());

            CreateMap<UserCreateDTO, UserModel>()
                .ForMember(x => x.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(x => x.URLs, opt => opt.MapFrom(src => new List<URLModel>()))
                .ForMember(x => x.URLCategories, opt => opt.MapFrom(src => new List<URLCategoryModel>()));

            CreateMap<UserUpdateDTO, UserModel>();
        }
    }
    internal class UserURLsResolver(IMapper mapper) : IValueResolver<UserModel, UserDTO, List<URLDTO>?>
    {
        private readonly IMapper _mapper = mapper;

        public List<URLDTO>? Resolve(UserModel source, UserDTO destination, List<URLDTO>? destMember, ResolutionContext context)
        {
            return source.URLs?.Select(x => _mapper.Map<URLDTO>(x)).ToList();
        }
    }
    internal class UserCategoriesResolver(IMapper mapper) : IValueResolver<UserModel, UserDTO, List<CategoryDTO>?>
    {
        private readonly IMapper _mapper = mapper;

        public List<CategoryDTO>? Resolve(UserModel source, UserDTO destination, List<CategoryDTO>? destMember, ResolutionContext context)
        {
            return source.URLCategories?.Select(x => _mapper.Map<CategoryDTO>(x)).ToList();
        }
    }
}