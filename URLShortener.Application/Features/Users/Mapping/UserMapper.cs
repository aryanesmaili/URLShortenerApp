using AutoMapper;
using URLShortener.Application.Features.Users.DTOs;
using URLShortener.Domain.Entities.User;

namespace URLShortener.Application.Features.Users.Mapping;

public sealed class UserMapper : Profile
{
    public UserMapper()
    {
        CreateMap<UserModel, UserDTO>()
            .ReverseMap();

        CreateMap<UserCreateDTO, UserModel>();

        CreateMap<UserUpdateDTO, UserModel>();
    }
}