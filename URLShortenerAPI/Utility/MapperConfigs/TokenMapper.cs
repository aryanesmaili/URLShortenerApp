using AutoMapper;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.User;

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
