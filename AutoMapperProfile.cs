using AutoMapper;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Bank, BankResponse>();
        CreateMap<BankRequest, Bank>();

        CreateMap<BankTransaction, BankTransactionResponse>()
            .ForMember(dest => dest.BankName, act => act.MapFrom(src => src.Bank.Name))
            .ForMember(dest => dest.ClientName, act => act.MapFrom(src => src.Client.Name));
        CreateMap<BankTransactionRequest, BankTransaction>();

        CreateMap<(BankTransaction from, BankTransaction to), (BankTransactionResponse from, BankTransactionResponse to
                )>()
            .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
            .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));

        CreateMap<Client, ClientResponse>();
        CreateMap<ClientRequest, Client>();

        CreateMap<ClosingNickname, ClosingNicknameResponse>();
        CreateMap<ClosingNicknameRequest, ClosingNickname>();

        CreateMap<Ofx, OfxResponse>();
        CreateMap<OfxRequest, Ofx>();


        CreateMap<Manager, ManagerResponse>();
        CreateMap<ManagerRequest, Manager>();

        CreateMap<Wallet, WalletResponse>();
        CreateMap<WalletRequest, Wallet>();

        CreateMap<Nickname, NicknameResponse>();
        CreateMap<NicknameRequest, Nickname>();

        CreateMap<WalletTransaction, WalletTransactionResponse>();
        CreateMap<WalletTransactionRequest, WalletTransaction>();

        CreateMap<(WalletTransaction from, WalletTransaction to), (WalletTransactionResponse from,
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