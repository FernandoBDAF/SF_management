using AutoMapper;
using SFManagement.Models;
using SFManagement.Models.Closing;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Address, AddressResponse>();
        CreateMap<AddressRequest, Address>();
        
        CreateMap<Bank, BankResponse>();
        CreateMap<BankRequest, Bank>();
        
        CreateMap<Client, ClientResponse>();
        CreateMap<ClientRequest, Client>();
        
        CreateMap<ContactPhone, ContactPhoneResponse>();
        CreateMap<ContactPhoneRequest, ContactPhone>();
        
        CreateMap<InitialBalance, InitialBalanceResponse>();
        CreateMap<InitialBalanceRequest, InitialBalance>();
        
        CreateMap<Member, MemberResponse>();
        CreateMap<MemberRequest, Member>();
        
        CreateMap<PokerManager, PokerManagerResponse>();
        CreateMap<PokerManagerRequest, PokerManager>();

        CreateMap<AssetWallet, AssetWalletResponse>()
            .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : null))
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member != null ? src.Member.Name : null))
            .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Bank != null ? src.Bank.Name : null))
            .ForMember(dest => dest.PokerManagerName, opt => opt.MapFrom(src => src.PokerManager != null ? src.PokerManager.Name : null));
        CreateMap<AssetWalletRequest, AssetWallet>();

        CreateMap<WalletIdentifier, WalletIdentifierResponse>();
        CreateMap<WalletIdentifierRequest, WalletIdentifier>();

        // CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>()
        //     .ForMember(dest => dest.BankName, act => act.MapFrom(src => src.Bank.Name))
        //     .ForMember(dest => dest.ClientName, act => act.MapFrom(src => src.Client.Name));
        // CreateMap<FiatAssetTransactionRequest, FiatAssetTransaction>();

        CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>();
        CreateMap<FiatAssetTransactionRequest, FiatAssetTransaction>();
        
        CreateMap<(FiatAssetTransaction from, FiatAssetTransaction to), (FiatAssetTransactionResponse from, FiatAssetTransactionResponse to
                )>()
            .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
            .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));
        
        CreateMap<ClosingNickname, ClosingNicknameResponse>();
        CreateMap<ClosingNicknameRequest, ClosingNickname>();

        CreateMap<Ofx, OfxResponse>();
        CreateMap<OfxRequest, Ofx>();

        CreateMap<OfxTransaction, OfxTransactionResponse>();
        CreateMap<OfxTransactionRequest, OfxTransaction>();
        
        CreateMap<DigitalAssetTransaction, DigitalAssetTransactionResponse>();
        CreateMap<DigitalAssetTransactionRequest, DigitalAssetTransaction>();

        CreateMap<(DigitalAssetTransaction from, DigitalAssetTransaction to), (DigitalAssetTransactionResponse from,
                DigitalAssetTransactionResponse to)>()
            .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
            .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));

        CreateMap<Excel, ExcelResponse>();
        CreateMap<ExcelRequest, Excel>();

        CreateMap<Tag, TagResponse>();
        CreateMap<TagRequest, Tag>();

        CreateMap<ClosingWallet, ClosingWalletResponse>();
        CreateMap<ClosingWalletRequest, ClosingWallet>();

        CreateMap<ClosingNickname, ClosingNicknameResponse>();
        CreateMap<ClosingNicknameRequest, ClosingNickname>();

        CreateMap<ClosingManager, ClosingManagerResponse>();
        CreateMap<ClosingManagerRequest, ClosingManager>();

        CreateMap<(InternalTransaction to, InternalTransaction from), (InternalTransactionResponse to,
            InternalTransactionResponse from)>();
        CreateMap<InternalTransaction, InternalTransactionResponse>();
        CreateMap<InternalTransactionRequest, InternalTransaction>();

        CreateMap<ApplicationUser, UserResponse>();

        CreateMap<AvgRate, AvgRateResponse>();
        CreateMap<AvgRateRequest, AvgRate>();
    }
}