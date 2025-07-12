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
                        
                    case WalletType.Internal:
                        // Internal wallets have no specific metadata fields to extract
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

        CreateMap<FiatAssetTransaction, FiatAssetTransactionResponse>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => "FiatAsset"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.SenderWalletIdentifier.AssetPool.AssetType))
            .ForMember(dest => dest.IsInternalTransfer, opt => opt.MapFrom(src => src.IsInternalTransfer))
            .AfterMap((src, dest, context) =>
            {
                // Map sender wallet summary
                dest.SenderWallet = MapWalletIdentifierSummary(src.SenderWalletIdentifier);
                
                // Map receiver wallet summary
                dest.ReceiverWallet = MapWalletIdentifierSummary(src.ReceiverWalletIdentifier);
                
                // Map OFX transaction if available
                if (src.OfxTransaction != null)
                {
                    dest.OfxTransaction = new OfxTransactionSummary
                    {
                        Id = src.OfxTransaction.Id,
                        FitId = src.OfxTransaction.FitId,
                        Value = src.OfxTransaction.Value,
                        Date = src.OfxTransaction.Date,
                        Description = src.OfxTransaction.Description,
                        BankName = src.OfxTransaction.Ofx?.Bank?.BaseAssetHolder?.Name,
                        FileName = src.OfxTransaction.Ofx?.FileName
                    };
                }
                
                // Map bank info from sender or receiver (prioritize sender)
                var bankWallet = src.SenderWalletIdentifier.WalletType == WalletType.BankWallet 
                    ? src.SenderWalletIdentifier 
                    : src.ReceiverWalletIdentifier.WalletType == WalletType.BankWallet 
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
            });
            
        CreateMap<FiatAssetTransactionRequest, FiatAssetTransaction>();

        CreateMap<DigitalAssetTransaction, DigitalAssetTransactionResponse>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => "DigitalAsset"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.SenderWalletIdentifier.AssetPool.AssetType))
            .ForMember(dest => dest.IsInternalTransfer, opt => opt.MapFrom(src => src.IsInternalTransfer))
            .AfterMap((src, dest, context) =>
            {
                // Map sender wallet summary
                dest.SenderWallet = MapWalletIdentifierSummary(src.SenderWalletIdentifier);
                
                // Map receiver wallet summary
                dest.ReceiverWallet = MapWalletIdentifierSummary(src.ReceiverWalletIdentifier);
                
                // Map Excel transaction if available
                if (src.Excel != null)
                {
                    dest.Excel = new ExcelTransactionSummary
                    {
                        Id = src.Excel.Id,
                        FileName = src.Excel.FileName,
                        FileType = src.Excel.FileType,
                        PokerManagerName = src.Excel.PokerManager?.BaseAssetHolder?.Name,
                        ImportedAt = src.Excel.CreatedAt ?? DateTime.UtcNow
                    };
                }
                
                // Map poker info from sender or receiver (prioritize sender)
                var pokerWallet = src.SenderWalletIdentifier.WalletType == WalletType.PokerWallet 
                    ? src.SenderWalletIdentifier 
                    : src.ReceiverWalletIdentifier.WalletType == WalletType.PokerWallet 
                        ? src.ReceiverWalletIdentifier 
                        : null;
                        
                if (pokerWallet != null)
                {
                    dest.PokerInfo = new PokerTransactionInfo
                    {
                        PlayerNickname = pokerWallet.GetPokerMetadata(PokerWalletMetadata.PlayerNickname),
                        PlayerEmail = pokerWallet.GetPokerMetadata(PokerWalletMetadata.PlayerEmail),
                        AccountStatus = pokerWallet.GetPokerMetadata(PokerWalletMetadata.AccountStatus),
                        PokerSite = GetPokerSiteFromAssetType(pokerWallet.AssetPool.AssetType)
                    };
                }
                
                // Map crypto info from sender or receiver (prioritize sender)
                var cryptoWallet = src.SenderWalletIdentifier.WalletType == WalletType.CryptoWallet 
                    ? src.SenderWalletIdentifier 
                    : src.ReceiverWalletIdentifier.WalletType == WalletType.CryptoWallet 
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
                        FromAsset = src.SenderWalletIdentifier.AssetPool.AssetType,
                        ToAsset = src.BalanceAs.Value,
                        FromAmount = src.AssetAmount,
                        ToAmount = src.AssetAmount * src.ConversionRate.Value,
                        ExchangeRate = src.ConversionRate.Value
                    };
                }
            });
            
        CreateMap<DigitalAssetTransactionRequest, DigitalAssetTransaction>();

        // CreateMap<(FiatAssetTransaction from, FiatAssetTransaction to), (FiatAssetTransactionResponse from, FiatAssetTransactionResponse to
        //         )>()
        //     .ForMember(dest => dest.from, opt => opt.MapFrom(src => src.from))
        //     .ForMember(dest => dest.to, opt => opt.MapFrom(src => src.to));
        

        CreateMap<Ofx, OfxResponse>()
            .AfterMap((src, dest, context) =>
            {
                // Map bank summary
                if (src.Bank?.BaseAssetHolder != null)
                {
                    dest.Bank = new BankSummary
                    {
                        Id = src.Bank.Id,
                        Name = src.Bank.BaseAssetHolder.Name,
                        Code = src.Bank.Code,
                        Email = src.Bank.BaseAssetHolder.Email
                    };
                }
                
                // Calculate file statistics
                var transactions = src.OfxTransactions?.ToList() ?? new List<OfxTransaction>();
                dest.Statistics = new OfxFileStatistics
                {
                    TotalTransactions = transactions.Count,
                    ProcessedTransactions = transactions.Count, // All loaded transactions are processed
                    SkippedTransactions = 0, // Would need to be calculated during import
                    TotalValue = transactions.Sum(t => Math.Abs(t.Value)),
                    TotalCredits = transactions.Where(t => t.Value > 0).Sum(t => t.Value),
                    TotalDebits = Math.Abs(transactions.Where(t => t.Value < 0).Sum(t => t.Value)),
                    EarliestTransactionDate = transactions.Count > 0 ? transactions.Min(t => t.Date) : null,
                    LatestTransactionDate = transactions.Count > 0 ? transactions.Max(t => t.Date) : null
                };
                
                // Map import info
                dest.ImportInfo = new OfxImportInfo
                {
                    ImportedAt = src.CreatedAt ?? DateTime.UtcNow,
                    ImportedBy = src.LastModifiedBy,
                    ProcessingStatus = "Completed",
                    ImportWarnings = new List<string>()
                };
            });
        CreateMap<OfxRequest, Ofx>();

        CreateMap<OfxTransaction, OfxTransactionResponse>()
            .AfterMap((src, dest, context) =>
            {
                // Map OFX file summary
                if (src.Ofx != null)
                {
                    dest.OfxFile = new OfxFileSummary
                    {
                        Id = src.Ofx.Id,
                        FileName = src.Ofx.FileName,
                        ImportedAt = src.Ofx.CreatedAt ?? DateTime.UtcNow
                    };
                }
                
                // Map bank summary
                if (src.Ofx?.Bank?.BaseAssetHolder != null)
                {
                    dest.Bank = new BankSummary
                    {
                        Id = src.Ofx.Bank.Id,
                        Name = src.Ofx.Bank.BaseAssetHolder.Name,
                        Code = src.Ofx.Bank.Code,
                        Email = src.Ofx.Bank.BaseAssetHolder.Email
                    };
                }
                
                // Check if there's a linked fiat asset transaction
                var linkedFiatTransaction = context.Items.ContainsKey("LinkedFiatTransactions") 
                    ? ((Dictionary<Guid, FiatAssetTransaction>)context.Items["LinkedFiatTransactions"]).GetValueOrDefault(src.Id)
                    : null;
                    
                if (linkedFiatTransaction != null)
                {
                    dest.LinkedTransaction = new FiatAssetTransactionSummary
                    {
                        Id = linkedFiatTransaction.Id,
                        Date = linkedFiatTransaction.Date,
                        AssetAmount = linkedFiatTransaction.AssetAmount,
                        Description = linkedFiatTransaction.Description,
                        ApprovedAt = linkedFiatTransaction.ApprovedAt,
                        IsInternalTransfer = linkedFiatTransaction.IsInternalTransfer,
                        CategoryName = linkedFiatTransaction.Category?.Description
                    };
                }
                
                // Map transaction classification
                dest.Classification = new OfxTransactionClassification
                {
                    IsProcessed = linkedFiatTransaction != null,
                    DetectedType = DetermineTransactionType(src.Description),
                    IsPotentialPix = IsLikelyPixTransaction(src.Description),
                    IsPotentialRecurring = false, // Would need historical analysis
                    ProcessingNotes = new List<string>()
                };
                
                // Add processing notes based on analysis
                if (dest.Classification.IsPotentialPix)
                {
                    dest.Classification.ProcessingNotes.Add("Detected as potential PIX transaction");
                }
                
                if (Math.Abs(src.Value) > 10000)
                {
                    dest.Classification.ProcessingNotes.Add("High-value transaction requiring attention");
                }
            });
        CreateMap<OfxTransactionRequest, OfxTransaction>();
        
        CreateMap<Excel, ExcelResponse>();
        CreateMap<ExcelRequest, Excel>();

        CreateMap<Category, CategoryResponse>();
        CreateMap<CategoryRequest, Category>();

        CreateMap<SettlementTransaction, SettlementTransactionResponse>()
            .ForMember(dest => dest.TransactionType, opt => opt.MapFrom(src => "Settlement"))
            .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.SenderWalletIdentifier.AssetPool.AssetType))
            .ForMember(dest => dest.IsInternalTransfer, opt => opt.MapFrom(src => src.IsInternalTransfer))
            .AfterMap((src, dest, context) =>
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
    
    /// <summary>
    /// Helper method to map wallet identifier to summary
    /// </summary>
    private static WalletIdentifierSummary MapWalletIdentifierSummary(WalletIdentifier walletIdentifier)
    {
        var summary = new WalletIdentifierSummary
        {
            Id = walletIdentifier.Id,
            WalletType = walletIdentifier.WalletType,
            AccountClassification = walletIdentifier.AccountClassification,
            AssetType = walletIdentifier.AssetPool.AssetType
        };

        // Map asset holder information (null for company pools)
        if (walletIdentifier.AssetPool.BaseAssetHolder != null)
        {
            summary.AssetHolder = new AssetHolderSummary
            {
                Id = walletIdentifier.AssetPool.BaseAssetHolder.Id,
                Name = walletIdentifier.AssetPool.BaseAssetHolder.Name,
                AssetHolderType = walletIdentifier.AssetPool.BaseAssetHolder.AssetHolderType,
                Email = walletIdentifier.AssetPool.BaseAssetHolder.Email
            };
        }

        // Set display metadata based on wallet type
        summary.DisplayMetadata = walletIdentifier.WalletType switch
        {
            WalletType.BankWallet => walletIdentifier.GetBankMetadata(BankWalletMetadata.AccountNumber),
            WalletType.PokerWallet => walletIdentifier.GetPokerMetadata(PokerWalletMetadata.PlayerNickname),
            WalletType.CryptoWallet => walletIdentifier.GetCryptoMetadata(CryptoWalletMetadata.WalletAddress),
            WalletType.Internal => "Internal Wallet",
            _ => null
        };

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