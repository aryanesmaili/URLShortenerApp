using AutoMapper;
using URLShortener.Application.Features.Finance.DTOs;
using URLShortener.Application.Features.Finance.DTOs.Zibal;
using URLShortener.Domain.Entities.Finance;
using URLShortener.Domain.ValueObjects.Payment;

namespace URLShortener.Application.Features.Finance.Mapping;

public sealed class PaymentConfigs : Profile
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
