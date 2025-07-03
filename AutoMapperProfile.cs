using AutoMapper;
using SFManagement.Models;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Address, AddressResponse>();
        CreateMap<AddressRequest, Address>();
        
        // Base Asset Holder mappings - reusable configuration
        CreateBaseAssetHolderMappings();
        
        CreateMap<BankRequest, Bank>();
        CreateMap<ClientRequest, Client>();
        CreateMap<MemberRequest, Member>();
        CreateMap<PokerManagerRequest, PokerManager>();
        
        CreateMap<ContactPhone, ContactPhoneResponse>();
        CreateMap<ContactPhoneRequest, ContactPhone>();
        
        CreateMap<InitialBalance, InitialBalanceResponse>();
        CreateMap<InitialBalanceRequest, InitialBalance>();

        CreateMap<AssetWallet, AssetWalletResponse>()
            .ForMember(dest => dest.BaseAssetHolderName,
                opt =>
                    opt.MapFrom(src => src.BaseAssetHolder.Name));
        CreateMap<AssetWalletRequest, AssetWallet>();

        CreateMap<WalletIdentifier, WalletIdentifierResponse>()
            .ForMember(dest => dest.BaseAssetHolderName,
                opt =>
                    opt.MapFrom(src => src.BaseAssetHolder.Name));
        CreateMap<WalletIdentifierRequest, WalletIdentifier>();

        CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>();
            // .ForMember(dest => dest.ClientNameAw, act => act.MapFrom(src => src.AssetWallet.Client.Name))   
            // .ForMember(dest => dest.ClientNameWi, act => act.MapFrom(src => src.WalletIdentifier.Client.Name));
            
        CreateMap<FiatAssetTransactionRequest, FiatAssetTransaction>();

        // CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>();
        // CreateMap<FiatAssetTransactionRequest, FiatAssetTransaction>();
        
        // CreateMap<(FiatAssetTransaction from, FiatAssetTransaction to), (FiatAssetTransactionResponse from, FiatAssetTransactionResponse to
        //         )>()
        //     .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
        //     .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));
        

        CreateMap<Ofx, OfxResponse>();
        CreateMap<OfxRequest, Ofx>();

        CreateMap<OfxTransaction, OfxTransactionResponse>();
        CreateMap<OfxTransactionRequest, OfxTransaction>();
        
        CreateMap<DigitalAssetTransaction, DigitalAssetTransactionResponse>();
        CreateMap<DigitalAssetTransactionRequest, DigitalAssetTransaction>();

        // CreateMap<(DigitalAssetTransaction from, DigitalAssetTransaction to), (DigitalAssetTransactionResponse from,
        //         DigitalAssetTransactionResponse to)>()
            // .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
            // .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));

        CreateMap<Excel, ExcelResponse>();
        CreateMap<ExcelRequest, Excel>();

        CreateMap<FinancialBehavior, FinancialBehaviorResponse>();
        CreateMap<FinancialBehaviorRequest, FinancialBehavior>();

        CreateMap<SettlementTransaction, SettlementTransactionResponse>();
        CreateMap<SettlementTransactionRequest, SettlementTransaction>();

        CreateMap<ApplicationUser, UserResponse>();
    }
    
    private void CreateBaseAssetHolderMappings()
    {
        // Bank mappings
        CreateMap<Bank, BankResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.ToString()))
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.MapFrom(src => src.BaseAssetHolder.WalletIdentifiers))
            .ForMember(dest => dest.AssetWallets, opt => opt.MapFrom(src => src.BaseAssetHolder.AssetWallets))
            .ForMember(dest => dest.InitialBalances, opt => opt.MapFrom(src => src.BaseAssetHolder.InitialBalances))
            .ForMember(dest => dest.ContactPhones, opt => opt.MapFrom(src => src.BaseAssetHolder.ContactPhones));

        // Client mappings
        CreateMap<Client, ClientResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address))
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.MapFrom(src => src.BaseAssetHolder.WalletIdentifiers))
            .ForMember(dest => dest.AssetWallets, opt => opt.MapFrom(src => src.BaseAssetHolder.AssetWallets))
            .ForMember(dest => dest.InitialBalances, opt => opt.MapFrom(src => src.BaseAssetHolder.InitialBalances))
            .ForMember(dest => dest.ContactPhones, opt => opt.MapFrom(src => src.BaseAssetHolder.ContactPhones));
            
        // Member mappings
        CreateMap<Member, MemberResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address))
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.MapFrom(src => src.BaseAssetHolder.WalletIdentifiers))
            .ForMember(dest => dest.Wallets, opt => opt.MapFrom(src => src.BaseAssetHolder.AssetWallets))
            .ForMember(dest => dest.InitialBalances, opt => opt.MapFrom(src => src.BaseAssetHolder.InitialBalances))
            .ForMember(dest => dest.ContactPhones, opt => opt.MapFrom(src => src.BaseAssetHolder.ContactPhones));
            
        // PokerManager mappings
        CreateMap<PokerManager, PokerManagerResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address))
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.MapFrom(src => src.BaseAssetHolder.WalletIdentifiers))
            .ForMember(dest => dest.AssetWallets, opt => opt.MapFrom(src => src.BaseAssetHolder.AssetWallets))
            .ForMember(dest => dest.InitialBalances, opt => opt.MapFrom(src => src.BaseAssetHolder.InitialBalances))
            .ForMember(dest => dest.ContactPhones, opt => opt.MapFrom(src => src.BaseAssetHolder.ContactPhones));
        
        CreateMap<BaseAssetHolder, BaseAssetHolderResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.MapFrom(src => src.WalletIdentifiers))
            .ForMember(dest => dest.AssetWallets, opt => opt.MapFrom(src => src.AssetWallets))
            .ForMember(dest => dest.InitialBalances, opt => opt.MapFrom(src => src.InitialBalances))
            .ForMember(dest => dest.ContactPhones, opt => opt.MapFrom(src => src.ContactPhones));
    }
}