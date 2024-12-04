using AutoMapper;
using SFManagement.Models;
using SFManagement.ViewModels;

namespace SFManagement
{
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

            CreateMap<Client, ClientResponse>();
            CreateMap<ClientRequest, Client>();

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

            CreateMap<Excel, ExcelResponse>();
            CreateMap<ExcelRequest, Excel>();

            CreateMap<Tag, TagResponse>();
            CreateMap<TagRequest, Tag>();

            CreateMap<ApplicationUser, UserResponse>();
		}
    }
}
