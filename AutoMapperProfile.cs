using AutoMapper;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Models.Entities;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;
using SFManagement.Enums;
using SFManagement.Enums.WalletsMetadata;

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

        CreateMap<AssetPool, AssetPoolResponse>()
            .ForMember(dest => dest.BaseAssetHolderName,
                opt =>
                    opt.MapFrom(src => src.BaseAssetHolder.Name));
        CreateMap<AssetPoolRequest, AssetPool>();

        CreateMap<WalletIdentifier, WalletIdentifierResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.AssetPool.BaseAssetHolder.Id))
            .ForMember(dest => dest.BaseAssetHolderName, opt => opt.MapFrom(src => src.AssetPool.BaseAssetHolder.Name))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetPool.AssetType))
            // .ForMember(dest => dest.ReferralId, opt => opt.MapFrom(src => src.Referral.Id))
            .AfterMap((src, dest, context) =>
            {
                // Extract metadata fields based on wallet type
                switch (src.WalletType)
                {
                    case WalletType.PokerWallet:
                        dest.InputForTransactions = src.GetPokerMetadata(PokerWalletMetadata.InputForTransactions);
                        dest.PlayerNickname = src.GetPokerMetadata(PokerWalletMetadata.PlayerNickname);
                        dest.PlayerEmail = src.GetPokerMetadata(PokerWalletMetadata.PlayerEmail);
                        dest.AccountStatus = src.GetPokerMetadata(PokerWalletMetadata.AccountStatus);
                        break;
                        
                    case WalletType.BankWallet:
                        dest.PixKey = src.GetBankMetadata(BankWalletMetadata.PixKey);
                        dest.AccountType = src.GetBankMetadata(BankWalletMetadata.AccountType);
                        dest.RoutingNumber = src.GetBankMetadata(BankWalletMetadata.RoutingNumber);
                        dest.AccountNumber = src.GetBankMetadata(BankWalletMetadata.AccountNumber);
                        break;
                        
                    case WalletType.CryptoWallet:
                        dest.WalletAddress = src.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress);
                        dest.WalletCategory = src.GetCryptoMetadata(CryptoWalletMetadata.WalletCategory);
                        break;
                }
            });
        CreateMap<WalletIdentifierRequest, WalletIdentifier>()
            .AfterMap((src, dest, context) =>
            {
                // If individual metadata fields are provided, use them to construct the metadata
                // This takes precedence over the raw MetadataJson
                if (HasIndividualMetadataFields(src))
                {
                    dest.SetMetadataFromFields(
                        inputForTransactions: src.InputForTransactions,
                        playerNickname: src.PlayerNickname,
                        playerEmail: src.PlayerEmail,
                        accountStatus: src.AccountStatus,
                        accountNumber: src.AccountNumber,
                        routingNumber: src.RoutingNumber,
                        walletAddress: src.WalletAddress,
                        walletCategory: src.WalletCategory,
                        pixKey: src.PixKey,
                        accountType: src.AccountType
                    );
                }
            });

        CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>();
            // .ForMember(dest => dest.ClientNameAw, act => act.MapFrom(src => src.AssetPool.Client.Name))   
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

        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();

        CreateMap<SettlementTransaction, SettlementTransactionResponse>();
        CreateMap<SettlementTransactionRequest, SettlementTransaction>();
        CreateMap<SettlementTransaction, SettlementTransactionSimplifiedResponse>();

        // Note: ApplicationUser and UserResponse mappings removed as they're replaced by Auth0
    }
    
    /// <summary>
    /// Helper method to check if any individual metadata fields are provided
    /// </summary>
    private static bool HasIndividualMetadataFields(WalletIdentifierRequest request)
    {
        return !string.IsNullOrEmpty(request.InputForTransactions) ||
               !string.IsNullOrEmpty(request.PlayerNickname) ||
               !string.IsNullOrEmpty(request.PlayerEmail) ||
               !string.IsNullOrEmpty(request.AccountStatus) ||
               !string.IsNullOrEmpty(request.PixKey) ||
               !string.IsNullOrEmpty(request.AccountType) ||
               !string.IsNullOrEmpty(request.AccountNumber) ||
               !string.IsNullOrEmpty(request.RoutingNumber) ||
               !string.IsNullOrEmpty(request.WalletAddress) ||
               !string.IsNullOrEmpty(request.WalletCategory);
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
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.ToString()));
            // Removed collection mappings - these properties no longer exist in response models

        // Client mappings
        CreateMap<Client, ClientResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address))
            .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday));
            // Removed collection mappings - these properties no longer exist in response models
            
        // Member mappings
        CreateMap<Member, MemberResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address))
            .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday))
            .ForMember(dest => dest.Share, opt => opt.MapFrom(src => src.Share));
            // Removed collection mappings - these properties no longer exist in response models
            
        // PokerManager mappings
        CreateMap<PokerManager, PokerManagerResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.BaseAssetHolder.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.BaseAssetHolder.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.BaseAssetHolder.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder.Address));
        
        CreateMap<BaseAssetHolder, BaseAssetHolderResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.Cpf, opt => opt.MapFrom(src => src.Cpf))
            .ForMember(dest => dest.Cnpj, opt => opt.MapFrom(src => src.Cnpj))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address));
            // Removed collection mappings - these properties no longer exist in response models
    }
}