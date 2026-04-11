using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.User;
using URLShortener.Domain.Entities.User;

namespace URLShortenerAPI.Responses.MapperConfigs;

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