using AutoMapper;
using SharedDataModels.DTOs;
using URLShortenerAPI.Data.Entities.Finance;

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
