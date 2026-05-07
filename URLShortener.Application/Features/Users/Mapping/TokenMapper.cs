using AutoMapper;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Application.Features.Users.Mapping;

public sealed class TokenMapper : Profile
{
    public TokenMapper()
    {
        CreateMap<RefreshToken, RefreshTokenDTO>()
            .ReverseMap();
    }
}
