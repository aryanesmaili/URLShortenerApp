using AutoMapper;
using URLShortener.Application.DTOs.EntityDTOs.Finance;
using URLShortener.Application.DTOs.ZibalDTOs;
using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortenerAPI.Utility.MapperConfigs;

public class PaymentConfigs : Profile
{
    public PaymentConfigs()
    {
        CreateMap<DepositModel, DepositDTO>()
            .ReverseMap();

        #region PaymentStrategy Mappings

        #region ZibalMappings
        CreateMap<PaymentCreateRequest, ZibalCreateTransactionRequest>();
        CreateMap<PaymentVerifyRequest, ZibalVerifyTransactionRequest>();
        CreateMap<PaymentStatusRequest, ZibalInquiryTransactionRequest>();
        CreateMap<ZibalCreateTransactionResponse, PaymentCreateResult>()
            .ForMember(dest => dest.Success, action => action.MapFrom(x => x.Result == 100));
        CreateMap<ZibalVerifyTransactionResponse, PaymentVerifyResult>()
            .ForMember(dest => dest.Success, action => action.MapFrom(x => x.Result == 100));
        CreateMap<ZibalInquiryTransactionResponse, PaymentStatusResult>()
            .ForMember(dest => dest.Success, action => action.MapFrom(x => x.Result == 100));

        #endregion

        #endregion
    }
}
