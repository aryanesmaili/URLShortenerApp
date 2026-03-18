using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.Finance;
using URLShortener.Domain.Entities.Finance;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    public class PaymentConfigs : Profile
    {
        public PaymentConfigs()
        {
            CreateMap<DepositModel, DepositDTO>()
                .ReverseMap();
        }
    }
}
