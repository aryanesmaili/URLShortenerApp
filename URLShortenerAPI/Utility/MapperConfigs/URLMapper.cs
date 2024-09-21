using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entites.ClickInfo;
using URLShortenerAPI.Data.Entites.URL;
using URLShortenerAPI.Data.Entites.User;
using URLShortenerAPI.Services.Interfaces;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    internal class URLMapper : Profile
    {
        public URLMapper()
        {
            CreateMap<URLModel, URLDTO>();
            CreateMap<URLCreateDTO, URLModel>()
                .ForMember(u => u.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(u => u.IsActive, opt => opt.MapFrom(src => true))
                .ForMember(u => u.ShortCode, opt => opt.MapFrom<ShortURLResolver>())
                .ForMember(u => u.LongURL, opt => opt.MapFrom(src => src.OriginalURL))
                .ForMember(u => u.User, opt => opt.MapFrom<URLUserResolver>())
                .ForMember(u => u.Clicks, opt => opt.MapFrom(src => new List<ClickInfoModel>()))
                ;

        }
    }

    internal class URLUserResolver : IValueResolver<URLCreateDTO, URLModel, UserModel>
    {
        private readonly AppDbContext _context;

        public URLUserResolver(AppDbContext context)
        {
            _context = context;
        }

        public UserModel Resolve(URLCreateDTO source, URLModel destination, UserModel destMember, ResolutionContext context)
        {
            return _context.Users.FirstOrDefault(u => u.ID == source.UserID) ?? throw new NotFoundException($"User {source.UserID} does not exist");
        }
    }

    internal class ShortURLResolver : IValueResolver<URLCreateDTO, URLModel, string>
    {
        private readonly IURLService _urlService;

        public ShortURLResolver(IURLService urlService)
        {
            _urlService = urlService;
        }

        public string Resolve(URLCreateDTO source, URLModel destination, string destMember, ResolutionContext context)
        {
            return _urlService.ShortURLGenerator(source.OriginalURL);
        }
    }
}
