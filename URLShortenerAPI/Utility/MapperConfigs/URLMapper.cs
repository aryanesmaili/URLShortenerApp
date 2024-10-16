using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Pexita.Utility.Exceptions;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data;
using URLShortenerAPI.Data.Entities.ClickInfo;
using URLShortenerAPI.Data.Entities.URL;
using URLShortenerAPI.Data.Entities.URLCategory;
using URLShortenerAPI.Data.Entities.User;
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
                .ForMember(u => u.Clicks, opt => opt.MapFrom(src => new List<ClickInfoModel>()))
                .ForMember(u => u.Categories, opt => opt.Ignore())
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
}
