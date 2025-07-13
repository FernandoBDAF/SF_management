using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.ViewModels;

namespace SFManagement.Examples;

/// <summary>
/// Examples demonstrating different WalletIdentifier creation scenarios
/// </summary>
public static class WalletIdentifierCreationScenarios
{
    /// <summary>
    /// SCENARIO 1: Create WalletIdentifier using existing AssetPoolId
    /// Use this when you already know which AssetPool to use
    /// </summary>
    public static WalletIdentifierRequest Scenario1_UseExistingAssetPool()
    {
        // You already have an AssetPool ID (e.g., from a previous query)
        var existingAssetPoolId = Guid.Parse("12345678-1234-1234-1234-123456789012");
        
        return new WalletIdentifierRequest
        {
            // SCENARIO 1: Provide existing AssetPoolId
            AssetPoolId = existingAssetPoolId,
            
            // AssetType is still required for validation
            AssetType = AssetType.PokerStars,
            AccountClassification = AccountClassification.ASSET,
            
            // Poker-specific metadata
            InputForTransactions = "pokerstars_player_123",
            PlayerNickname = "PokerPro2024",
            PlayerEmail = "player@example.com"
        };
    }
    
    /// <summary>
    /// SCENARIO 2: Create WalletIdentifier using BaseAssetHolderId + AssetType
    /// The service will find or create the appropriate AssetPool automatically
    /// </summary>
    public static WalletIdentifierRequest Scenario2_AutoCreateAssetPool()
    {
        // You have a BaseAssetHolder (e.g., a PokerManager) and want to create a wallet for a specific AssetType
        var pokerManagerId = Guid.Parse("87654321-4321-4321-4321-210987654321");
        
        return new WalletIdentifierRequest
        {
            // SCENARIO 2: Provide BaseAssetHolderId + AssetType
            BaseAssetHolderId = pokerManagerId,
            AssetType = AssetType.GgPoker, // Service will find/create PokerAssets AssetPool
            
            AccountClassification = AccountClassification.ASSET,
            
            // GGPoker-specific metadata
            InputForTransactions = "ggpoker_player_456",
            PlayerNickname = "GGPokerPro",
            PlayerEmail = "ggplayer@example.com"
        };
    }
    
    /// <summary>
    /// Example showing different AssetTypes and their automatic AssetGroup assignment
    /// </summary>
    public static class AssetTypeExamples
    {
        /// <summary>
        /// Fiat Asset Example - will create/use FiatAssets AssetPool
        /// </summary>
        public static WalletIdentifierRequest BrazilianRealWallet(Guid bankId)
        {
            return new WalletIdentifierRequest
            {
                BaseAssetHolderId = bankId,
                AssetType = AssetType.BrazilianReal, // → FiatAssets AssetGroup
                AccountClassification = AccountClassification.ASSET,
                
                // Bank-specific metadata
                PixKey = "user@example.com",
                AccountType = "Checking",
                AccountNumber = "12345-6",
                RoutingNumber = "001"
            };
        }
        
        /// <summary>
        /// Crypto Asset Example - will create/use CryptoAssets AssetPool
        /// </summary>
        public static WalletIdentifierRequest BitcoinWallet(Guid clientId)
        {
            return new WalletIdentifierRequest
            {
                BaseAssetHolderId = clientId,
                AssetType = AssetType.Bitcoin, // → CryptoAssets AssetGroup
                AccountClassification = AccountClassification.ASSET,
                
                // Crypto-specific metadata
                WalletAddress = "bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh",
                WalletCategory = "Hot"
            };
        }
        
        /// <summary>
        /// Poker Asset Example - will create/use PokerAssets AssetPool
        /// </summary>
        public static WalletIdentifierRequest PokerStarsWallet(Guid pokerManagerId)
        {
            return new WalletIdentifierRequest
            {
                BaseAssetHolderId = pokerManagerId,
                AssetType = AssetType.PokerStars, // → PokerAssets AssetGroup
                AccountClassification = AccountClassification.ASSET,
                
                // Poker-specific metadata
                InputForTransactions = "pokerstars_player_789",
                PlayerNickname = "StarPlayer",
                PlayerEmail = "star@example.com",
                AccountStatus = "Verified"
            };
        }
    }
    
    /// <summary>
    /// Example showing the validation that occurs during creation
    /// </summary>
    public static class ValidationExamples
    {
        /// <summary>
        /// ✅ Valid: AssetType matches AssetPool's AssetGroup
        /// </summary>
        public static void ValidCreation()
        {
            // If you provide an AssetPoolId that has AssetGroup = PokerAssets
            // And AssetType = PokerStars (which belongs to PokerAssets)
            // ✅ This will be valid
            
            var request = new WalletIdentifierRequest
            {
                AssetPoolId = Guid.NewGuid(), // Assume this AssetPool has AssetGroup.PokerAssets
                AssetType = AssetType.PokerStars, // ✅ Compatible with PokerAssets
                AccountClassification = AccountClassification.ASSET
            };
        }
        
        /// <summary>
        /// ❌ Invalid: AssetType doesn't match AssetPool's AssetGroup
        /// </summary>
        public static void InvalidCreation()
        {
            // If you provide an AssetPoolId that has AssetGroup = FiatAssets
            // And AssetType = Bitcoin (which belongs to CryptoAssets)
            // ❌ This will fail validation
            
            var request = new WalletIdentifierRequest
            {
                AssetPoolId = Guid.NewGuid(), // Assume this AssetPool has AssetGroup.FiatAssets
                AssetType = AssetType.Bitcoin, // ❌ Incompatible with FiatAssets (should be CryptoAssets)
                AccountClassification = AccountClassification.ASSET
            };
            
            // This would throw: "AssetType 'Bitcoin' is not compatible with AssetPool's AssetGroup 'FiatAssets'. Expected AssetGroup: 'CryptoAssets'"
        }
    }
    
    /// <summary>
    /// API Usage Examples
    /// </summary>
    public static class ApiUsageExamples
    {
        /// <summary>
        /// POST /api/v1/WalletIdentifier - Scenario 1
        /// </summary>
        public static object ApiScenario1()
        {
            return new
            {
                assetPoolId = "12345678-1234-1234-1234-123456789012",
                assetType = "PokerStars",
                accountClassification = "ASSET",
                inputForTransactions = "pokerstars_player_123",
                playerNickname = "PokerPro2024"
            };
        }
        
        /// <summary>
        /// POST /api/v1/WalletIdentifier - Scenario 2
        /// </summary>
        public static object ApiScenario2()
        {
            return new
            {
                baseAssetHolderId = "87654321-4321-4321-4321-210987654321",
                assetType = "Bitcoin",
                accountClassification = "ASSET",
                walletAddress = "bc1qxy2kgdygjrsqtzq2n0yrf2493p83kkfjhx0wlh",
                walletCategory = "Cold"
            };
        }
    }
} 