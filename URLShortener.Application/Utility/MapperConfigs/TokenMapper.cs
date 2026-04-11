using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Domain.Entities.User;

namespace URLShortenerAPI.Responses.MapperConfigs;

public sealed class TokenMapper : Profile
{
    public TokenMapper()
    {
        CreateMap<RefreshToken, RefreshTokenDTO>()
            .ReverseMap();
    }
}
