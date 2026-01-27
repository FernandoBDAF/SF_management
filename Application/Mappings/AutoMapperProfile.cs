using AutoMapper;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.CompanyAssets;
using SFManagement.Application.DTOs.ImportedTransactions;
using SFManagement.Application.DTOs.Support;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Enums.Metadata;

namespace SFManagement.Application.Mappings;

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
        
        CreateMap<InitialBalance, InitialBalanceResponse>()
            .ForMember(dest => dest.BaseAssetHolderName, opt => opt.MapFrom(src => src.BaseAssetHolder.Name))
            .ForMember(dest => dest.AssetTypeName, opt => opt.MapFrom(src => src.AssetType.ToString()))
            .ForMember(dest => dest.AssetGroupName, opt => opt.MapFrom(src => src.AssetGroup.ToString()))
            .ForMember(dest => dest.BalanceAsName, opt => opt.MapFrom(src => src.BalanceAs.HasValue ? src.BalanceAs.Value.ToString() : null))
            .ForMember(dest => dest.EffectiveBalance, opt => opt.MapFrom(src => src.EffectiveBalance))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        CreateMap<InitialBalanceRequest, InitialBalance>();

        CreateMap<AssetPool, AssetPoolResponse>()
            .ForMember(dest => dest.BaseAssetHolderName,
                opt =>
                    opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Name : "Company"));
        CreateMap<AssetPoolRequest, AssetPool>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolderId))
            .AfterMap((src, dest, context) =>
            {
                // Additional validation to prevent company pools through regular endpoint
                if (dest.BaseAssetHolderId == Guid.Empty)
                {
                    throw new ArgumentException("BaseAssetHolderId cannot be empty. For company pools, use the CompanyAssetPoolController.");
                }
                if (!dest.BaseAssetHolderId.HasValue)
                {
                    throw new ArgumentException("BaseAssetHolderId is required. For company pools, use the CompanyAssetPoolController.");
                }
            });

        // Company Asset Pool mappings
        CreateMap<CompanyAssetPoolRequest, AssetPool>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => (Guid?)null))
            .ForMember(dest => dest.BaseAssetHolder, opt => opt.Ignore())
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.Ignore());

        CreateMap<AssetPool, CompanyAssetPoolResponse>()
            .ForMember(dest => dest.WalletIdentifierCount, opt => opt.MapFrom(src => src.WalletIdentifiers.Count))
            .ForMember(dest => dest.CurrentBalance, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.TransactionCount, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.LastTransactionDate, opt => opt.Ignore()) // Will be set manually
            .ForMember(dest => dest.Description, opt => opt.Ignore()) // Future enhancement
            .ForMember(dest => dest.BusinessJustification, opt => opt.Ignore()) // Future enhancement
            .ForMember(dest => dest.WalletIdentifiers, opt => opt.MapFrom(src => src.WalletIdentifiers));

        CreateMap<WalletIdentifier, WalletIdentifierResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.AssetPool.BaseAssetHolder != null ? src.AssetPool.BaseAssetHolder.Id : (Guid?)null))
            .ForMember(dest => dest.BaseAssetHolderName, opt => opt.MapFrom(src => src.AssetPool.BaseAssetHolder != null ? src.AssetPool.BaseAssetHolder.Name : "Company"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType))
            .ForMember(dest => dest.AssetGroup, opt => opt.MapFrom(src => src.AssetGroup))
            // .ForMember(dest => dest.ReferralId, opt => opt.MapFrom(src => src.Referral.Id))
            .AfterMap((src, dest, context) =>
            {
                // Extract metadata fields based on wallet type
                switch (src.AssetGroup)
                {
                    case AssetGroup.PokerAssets:
                        dest.InputForTransactions = src.GetPokerMetadata(PokerWalletMetadata.InputForTransactions);
                        dest.PlayerNickname = src.GetPokerMetadata(PokerWalletMetadata.PlayerNickname);
                        dest.PlayerEmail = src.GetPokerMetadata(PokerWalletMetadata.PlayerEmail);
                        dest.PlayerPhone = src.GetPokerMetadata(PokerWalletMetadata.PlayerPhone);
                        dest.AccountStatus = src.GetPokerMetadata(PokerWalletMetadata.AccountStatus);
                        break;
                        
                    case AssetGroup.FiatAssets:
                        dest.BankName = src.GetBankMetadata(BankWalletMetadata.BankName);
                        dest.PixKey = src.GetBankMetadata(BankWalletMetadata.PixKey);
                        dest.AccountType = src.GetBankMetadata(BankWalletMetadata.AccountType);
                        dest.RoutingNumber = src.GetBankMetadata(BankWalletMetadata.RoutingNumber);
                        dest.AccountNumber = src.GetBankMetadata(BankWalletMetadata.AccountNumber);
                        break;
                        
                    case AssetGroup.CryptoAssets:
                        dest.WalletAddress = src.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress);
                        dest.WalletCategory = src.GetCryptoMetadata(CryptoWalletMetadata.WalletCategory);
                        break;
                        
                    case AssetGroup.Internal:
                        // Internal wallets have no specific metadata fields to extract
                        break;
                }
            });
        CreateMap<WalletIdentifierRequest, WalletIdentifier>()
            .AfterMap((src, dest, context) =>
            {
                // Always ensure MetadataJson is initialized
                if (string.IsNullOrEmpty(dest.MetadataJson))
                {
                    dest.MetadataJson = "{}";
                }
                
                // If individual metadata fields are provided, use them to construct the metadata
                // This takes precedence over the raw MetadataJson
                if (HasIndividualMetadataFields(src))
                {
                    dest.SetMetadataFromFields(
                        inputForTransactions: src.InputForTransactions,
                        playerNickname: src.PlayerNickname,
                        playerPhone: src.PlayerPhone,
                        playerEmail: src.PlayerEmail,
                        accountStatus: src.AccountStatus,
                        accountNumber: src.AccountNumber,
                        routingNumber: src.RoutingNumber,
                        walletAddress: src.WalletAddress,
                        walletCategory: src.WalletCategory,
                        pixKey: src.PixKey,
                        bankName: src.BankName,
                        accountType: src.AccountType
                    );
                }
                else
                {
                    // Even if no individual fields are provided, call SetMetadataFromFields 
                    // to ensure proper initialization based on AssetGroup
                    dest.SetMetadataFromFields();
                }
            });

        CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => "FiatAsset"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.SenderWalletIdentifier != null ? src.SenderWalletIdentifier.AssetType : AssetType.None))
            .ForMember(dest => dest.IsInternalTransfer, opt => opt.MapFrom(src => src.IsInternalTransfer))
            .AfterMap((src, dest, context) =>
            {
                // Add null checks for navigation properties
                if (src.SenderWalletIdentifier != null && src.ReceiverWalletIdentifier != null)
                {
                    // Map sender wallet summary
                    dest.SenderWallet = MapWalletIdentifierSummary(src.SenderWalletIdentifier);
                    
                    // Map receiver wallet summary
                    dest.ReceiverWallet = MapWalletIdentifierSummary(src.ReceiverWalletIdentifier);
                    
                    // Map bank info from sender or receiver (prioritize sender)
                    var bankWallet = src.SenderWalletIdentifier.AssetGroup == AssetGroup.FiatAssets 
                        ? src.SenderWalletIdentifier 
                        : src.ReceiverWalletIdentifier.AssetGroup == AssetGroup.FiatAssets 
                            ? src.ReceiverWalletIdentifier 
                            : null;
                            
                    if (bankWallet != null)
                    {
                        dest.BankInfo = new BankTransactionInfo
                        {
                            BankName = bankWallet.AssetPool?.BaseAssetHolder?.Name,
                            AccountNumber = bankWallet.GetBankMetadata(BankWalletMetadata.AccountNumber),
                            RoutingNumber = bankWallet.GetBankMetadata(BankWalletMetadata.RoutingNumber),
                            AccountType = bankWallet.GetBankMetadata(BankWalletMetadata.AccountType)
                        };
                        
                        var pixKey = bankWallet.GetBankMetadata(BankWalletMetadata.PixKey);
                        if (!string.IsNullOrEmpty(pixKey))
                        {
                            dest.PixInfo = new PixTransactionInfo
                            {
                                PixKey = pixKey,
                                PixKeyType = DeterminePixKeyType(pixKey)
                            };
                        }
                    }
                }
                else
                {
                    // Set default values when navigation properties are not loaded
                    dest.SenderWallet = new WalletIdentifierSummary();
                    dest.ReceiverWallet = new WalletIdentifierSummary();
                }
            });
            
        CreateMap<FiatAssetTransactionRequest, FiatAssetTransaction>();

        CreateMap<DigitalAssetTransaction, DigitalAssetTransactionResponse>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => "DigitalAsset"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.SenderWalletIdentifier != null ? src.SenderWalletIdentifier.AssetType : AssetType.None))
            .ForMember(dest => dest.IsInternalTransfer, opt => opt.MapFrom(src => src.IsInternalTransfer))
            .AfterMap((src, dest, context) =>
            {
                // Add null checks for navigation properties
                if (src.SenderWalletIdentifier != null && src.ReceiverWalletIdentifier != null)
                {
                    // Map sender wallet summary
                    dest.SenderWallet = MapWalletIdentifierSummary(src.SenderWalletIdentifier);
                    
                    // Map receiver wallet summary
                    dest.ReceiverWallet = MapWalletIdentifierSummary(src.ReceiverWalletIdentifier);
                    
                    // Map poker info from sender or receiver (prioritize sender)
                    var pokerWallet = src.SenderWalletIdentifier.AssetGroup == AssetGroup.PokerAssets 
                        ? src.SenderWalletIdentifier 
                        : src.ReceiverWalletIdentifier.AssetGroup == AssetGroup.PokerAssets 
                            ? src.ReceiverWalletIdentifier 
                            : null;
                            
                    if (pokerWallet != null)
                    {
                        dest.PokerInfo = new PokerTransactionInfo
                        {
                            PlayerNickname = pokerWallet.GetPokerMetadata(PokerWalletMetadata.PlayerNickname),
                            PlayerEmail = pokerWallet.GetPokerMetadata(PokerWalletMetadata.PlayerEmail),
                            AccountStatus = pokerWallet.GetPokerMetadata(PokerWalletMetadata.AccountStatus),
                            PokerSite = GetPokerSiteFromAssetType(pokerWallet.AssetType)
                        };
                    }
                    
                    // Map crypto info from sender or receiver (prioritize sender)
                    var cryptoWallet = src.SenderWalletIdentifier.AssetGroup == AssetGroup.CryptoAssets 
                        ? src.SenderWalletIdentifier 
                        : src.ReceiverWalletIdentifier.AssetGroup == AssetGroup.CryptoAssets 
                            ? src.ReceiverWalletIdentifier 
                            : null;
                            
                    if (cryptoWallet != null)
                    {
                        dest.CryptoInfo = new CryptoTransactionInfo
                        {
                            WalletAddress = cryptoWallet.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress),
                            WalletCategory = cryptoWallet.GetCryptoMetadata(CryptoWalletMetadata.WalletCategory),
                            NetworkType = cryptoWallet.GetCryptoMetadata(CryptoWalletMetadata.NetworkType)
                        };
                    }
                    
                    // Map conversion details if this is a conversion transaction
                    if (src.BalanceAs.HasValue && src.ConversionRate.HasValue)
                    {
                        dest.ConversionDetails = new ConversionDetails
                        {
                            FromAsset = src.SenderWalletIdentifier.AssetType,
                            ToAsset = src.BalanceAs.Value,
                            FromAmount = src.AssetAmount,
                            ToAmount = src.AssetAmount * src.ConversionRate.Value,
                            ExchangeRate = src.ConversionRate.Value
                        };
                    }
                }
                else
                {
                    // Set default values when navigation properties are not loaded
                    dest.SenderWallet = new WalletIdentifierSummary();
                    dest.ReceiverWallet = new WalletIdentifierSummary();
                }
            });
            
        CreateMap<DigitalAssetTransactionRequest, DigitalAssetTransaction>();
        
        // CreateMap<(FiatAssetTransaction from, FiatAssetTransaction to), (FiatAssetTransactionResponse from, FiatAssetTransactionResponse to
        //         )>()
        //     .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
        //     .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));
        
        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();

        CreateMap<SettlementTransaction, SettlementTransactionResponse>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => "Settlement"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.SenderWalletIdentifier != null ? src.SenderWalletIdentifier.AssetType : AssetType.None))
            .ForMember(dest => dest.IsInternalTransfer, opt => opt.MapFrom(src => src.IsInternalTransfer))
            .AfterMap((src, dest, context) =>
            {
                // Add null checks for navigation properties
                if (src.SenderWalletIdentifier != null && src.ReceiverWalletIdentifier != null)
                {
                    // Map sender wallet summary
                    dest.SenderWallet = MapWalletIdentifierSummary(src.SenderWalletIdentifier);
                    
                    // Map receiver wallet summary
                    dest.ReceiverWallet = MapWalletIdentifierSummary(src.ReceiverWalletIdentifier);
                    
                    // Map settlement details
                    dest.SettlementInfo = new SettlementDetails
                    {
                        SettlementType = "Poker Settlement",
                        // Additional settlement details can be added here based on business requirements
                    };
                }
                else
                {
                    // Set default values when navigation properties are not loaded
                    dest.SenderWallet = new WalletIdentifierSummary();
                    dest.ReceiverWallet = new WalletIdentifierSummary();
                }
            });
            
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
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Id : Guid.Empty))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Name : string.Empty))
            .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.GovernmentNumber : string.Empty))
            .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.TaxEntityType : default))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Addresses.FirstOrDefault() : null))
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code.ToString()));
            // Removed collection mappings - these properties no longer exist in response models

        // Client mappings
        CreateMap<Client, ClientResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Id : Guid.Empty))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Name : string.Empty))
            .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.GovernmentNumber : string.Empty))
            .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.TaxEntityType : default))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Addresses.FirstOrDefault() : null))
            .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday));
            // Removed collection mappings - these properties no longer exist in response models
            
        // Member mappings
        CreateMap<Member, MemberResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Id : Guid.Empty))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Name : string.Empty))
            .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.GovernmentNumber : string.Empty))
            .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.TaxEntityType : default))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Addresses.FirstOrDefault() : null))
            .ForMember(dest => dest.Birthday, opt => opt.MapFrom(src => src.Birthday))
            .ForMember(dest => dest.Share, opt => opt.MapFrom(src => src.Share))
            .ForMember(dest => dest.Salary, opt => opt.MapFrom(src => src.Salary));
            // Removed collection mappings - these properties no longer exist in response models
            
        // PokerManager mappings
        CreateMap<PokerManager, PokerManagerResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Id : Guid.Empty))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Name : string.Empty))
            .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.GovernmentNumber : string.Empty))
            .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.TaxEntityType : default))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.Addresses.FirstOrDefault() : null))
            .ForMember(dest => dest.AssetPools, opt => opt.MapFrom(src => src.BaseAssetHolder != null ? src.BaseAssetHolder.AssetPools : null));
        
        CreateMap<BaseAssetHolder, BaseAssetHolderResponse>()
            .ForMember(dest => dest.BaseAssetHolderId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.GovernmentNumber, opt => opt.MapFrom(src => src.GovernmentNumber))
            .ForMember(dest => dest.TaxEntityType, opt => opt.MapFrom(src => src.TaxEntityType))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Addresses.FirstOrDefault()));
            // Removed collection mappings - these properties no longer exist in response models
    }
    
    /// <summary>
    /// Helper method to map wallet identifier to summary
    /// </summary>
    private static WalletIdentifierSummary MapWalletIdentifierSummary(WalletIdentifier walletIdentifier)
    {
        // Add null check for the walletIdentifier itself
        if (walletIdentifier == null)
        {
            return new WalletIdentifierSummary();
        }

        var summary = new WalletIdentifierSummary
        {
            Id = walletIdentifier.Id,
            AssetGroup = walletIdentifier.AssetGroup,
            AccountClassification = walletIdentifier.AccountClassification,
            AssetType = walletIdentifier.AssetType
        };

        // Map asset holder information (null for company pools)
        if (walletIdentifier.AssetPool?.BaseAssetHolder != null)
        {
            var baseAssetHolder = walletIdentifier.AssetPool.BaseAssetHolder;
            summary.AssetHolder = new AssetHolderSummary
            {
                Id = baseAssetHolder.Id,
                BaseAssetHolderId = baseAssetHolder.Id,
                Name = baseAssetHolder.Name,
                AssetHolderType = baseAssetHolder.AssetHolderType,
                Email = null
            };
        }

        // Set display metadata based on wallet type - add null checks for metadata methods
        try
        {
            summary.DisplayMetadata = walletIdentifier.AssetGroup switch
            {
                AssetGroup.FiatAssets => walletIdentifier.GetBankMetadata(BankWalletMetadata.PixKey),
                AssetGroup.PokerAssets => walletIdentifier.GetPokerMetadata(PokerWalletMetadata.InputForTransactions),
                AssetGroup.CryptoAssets => walletIdentifier.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress),
                AssetGroup.Internal => "Internal Wallet",
                _ => null
            };
        }
        catch
        {
            // If metadata access fails, set to null
            summary.DisplayMetadata = null;
        }

        return summary;
    }
    
    /// <summary>
    /// Helper method to determine PIX key type
    /// </summary>
    private static string? DeterminePixKeyType(string pixKey)
    {
        if (string.IsNullOrEmpty(pixKey))
            return null;
            
        // Email format
        if (pixKey.Contains('@'))
            return "Email";
            
        // Phone format (starts with +55 or just numbers)
        if (pixKey.StartsWith("+55") || (pixKey.All(char.IsDigit) && pixKey.Length >= 10))
            return "Phone";
            
        // CPF format (11 digits)
        if (pixKey.All(char.IsDigit) && pixKey.Length == 11)
            return "CPF";
            
        // CNPJ format (14 digits)
        if (pixKey.All(char.IsDigit) && pixKey.Length == 14)
            return "CNPJ";
            
        // Random key format (UUID-like)
        if (Guid.TryParse(pixKey, out _))
            return "Random";
            
        return "Unknown";
    }
    
    /// <summary>
    /// Helper method to get poker site name from asset type
    /// </summary>
    private static string? GetPokerSiteFromAssetType(AssetType assetType)
    {
        return assetType switch
        {
            AssetType.PokerStars => "PokerStars",
            AssetType.GgPoker => "GGPoker",
            AssetType.YaPoker => "YaPoker",
            AssetType.AmericasCardRoom => "Americas Cardroom",
            AssetType.SupremaPoker => "Suprema Poker",
            AssetType.AstroPayICash => "AstroPay iCash",
            AssetType.LuxonPoker => "Luxon Poker",
            _ => null
        };
    }
    
    /// <summary>
    /// Helper method to determine transaction type based on description
    /// </summary>
    private static string? DetermineTransactionType(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return null;
            
        var desc = description.ToUpperInvariant();
        
        // PIX transactions
        if (desc.Contains("PIX"))
            return "PIX Transfer";
            
        // Salary/payroll
        if (desc.Contains("SALARIO") || desc.Contains("SALARY") || desc.Contains("FOLHA"))
            return "Salary/Payroll";
            
        // Utility bills
        if (desc.Contains("CONTA") && (desc.Contains("LUZ") || desc.Contains("AGUA") || desc.Contains("GAS")))
            return "Utility Bill";
            
        // ATM/Cash withdrawal
        if (desc.Contains("SAQUE") || desc.Contains("ATM"))
            return "ATM Withdrawal";
            
        // Credit card payment
        if (desc.Contains("CARTAO") || desc.Contains("CARD"))
            return "Credit Card";
            
        // Transfer operations
        if (desc.Contains("TRANSFERENCIA") || desc.Contains("TRANSFER") || desc.Contains("TED") || desc.Contains("DOC"))
            return "Bank Transfer";
            
        // Investment operations
        if (desc.Contains("APLICACAO") || desc.Contains("RESGATE") || desc.Contains("INVESTIMENTO"))
            return "Investment";
            
        // Tax payments
        if (desc.Contains("IMPOSTO") || desc.Contains("TAXA") || desc.Contains("TARIFA"))
            return "Tax/Fee";
            
        // Rent payments
        if (desc.Contains("ALUGUEL") || desc.Contains("RENT"))
            return "Rent Payment";
            
        return "Other";
    }
    
    /// <summary>
    /// Helper method to detect if a transaction is likely a PIX transaction
    /// </summary>
    private static bool IsLikelyPixTransaction(string? description)
    {
        if (string.IsNullOrEmpty(description))
            return false;
            
        var desc = description.ToUpperInvariant();
        
        // Direct PIX indicators
        if (desc.Contains("PIX"))
            return true;
            
        // Common PIX patterns in Brazilian banking
        if (desc.Contains("TRANSFERENCIA INSTANTANEA") || 
            desc.Contains("INST PAYMENT") ||
            desc.Contains("PAGAMENTO INSTANTANEO"))
            return true;
            
        return false;
    }
}