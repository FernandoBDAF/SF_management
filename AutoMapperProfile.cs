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

        CreateMap<Wallet, WalletResponse>();
        CreateMap<WalletRequest, Wallet>();

        CreateMap<WalletIdentifier, NicknameResponse>();
        CreateMap<NicknameRequest, WalletIdentifier>();

        // CreateMap<FiatAssetTransaction, BankTransactionResponse>()
        //     .ForMember(dest => dest.BankName, act => act.MapFrom(src => src.Bank.Name))
        //     .ForMember(dest => dest.ClientName, act => act.MapFrom(src => src.Client.Name));
        // CreateMap<BankTransactionRequest, FiatAssetTransaction>();

        CreateMap<(FiatAssetTransaction from, FiatAssetTransaction to), (BankTransactionResponse from, BankTransactionResponse to
                )>()
            .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
            .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));
        
        CreateMap<ClosingNickname, ClosingNicknameResponse>();
        CreateMap<ClosingNicknameRequest, ClosingNickname>();

        CreateMap<Ofx, OfxResponse>();
        CreateMap<OfxRequest, Ofx>();

        CreateMap<OfxTransaction, OfxTransactionResponse>();
        CreateMap<OfxTransactionRequest, OfxTransaction>();
        
        CreateMap<DigitalAssetTransaction, WalletTransactionResponse>();
        CreateMap<WalletTransactionRequest, DigitalAssetTransaction>();

        CreateMap<(DigitalAssetTransaction from, DigitalAssetTransaction to), (WalletTransactionResponse from,
                WalletTransactionResponse to)>()
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