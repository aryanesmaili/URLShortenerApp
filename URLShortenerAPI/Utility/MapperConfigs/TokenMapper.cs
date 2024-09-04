using AutoMapper;
using URLShortenerAPI.Data.Entites.User;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    internal class TokenMapper : Profile
    {
        public TokenMapper()
        {
            CreateMap<RefreshToken, RefreshTokenDTO>();
        }
    }
}
