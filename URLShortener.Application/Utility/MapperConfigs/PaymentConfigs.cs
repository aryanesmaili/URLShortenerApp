using AutoMapper;
using URLShortener.Application.DTOs;
using URLShortener.Domain.Entities.Finance;

namespace URLShortenerAPI.Utility.MapperConfigs
{
    public class PaymentConfigs : Profile
    {
        public PaymentConfigs()
        {
            CreateMap<DepositModel, DepositDTO>();
        }
    }
}
