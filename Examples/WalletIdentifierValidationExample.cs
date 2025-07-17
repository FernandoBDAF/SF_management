using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Services;
using SFManagement.Enums.AssetInfrastructure;
namespace SFManagement.Examples;

/// <summary>
/// Example demonstrating AssetType/AssetGroup validation in WalletIdentifier creation
/// </summary>
public static class WalletIdentifierValidationExample
{
    /// <summary>
    /// Example of valid WalletIdentifier creation - AssetType matches AssetPool's AssetGroup
    /// </summary>
    public static void ValidExample()
    {
        // Create an AssetPool for PokerAssets
        var pokerAssetPool = new AssetPool
        {
            Id = Guid.NewGuid(),
            BaseAssetHolderId = Guid.NewGuid(),
            AssetGroup = AssetGroup.PokerAssets
        };
        
        // Create a WalletIdentifier with PokerStars AssetType (which belongs to PokerAssets group)
        var walletIdentifier = new WalletIdentifier
        {
            Id = Guid.NewGuid(),
            AssetPoolId = pokerAssetPool.Id,
            AssetPool = pokerAssetPool,
            AssetType = AssetType.PokerStars, // ✅ Valid: PokerStars belongs to PokerAssets group
            AccountClassification = AccountClassification.ASSET
        };
        
        // Validate compatibility
        var validator = new WalletIdentifierValidationService();
        var result = validator.ValidateWalletIdentifier(walletIdentifier);
        
        Console.WriteLine($"Validation Result: {(result.IsValid ? "✅ VALID" : "❌ INVALID")}");
        
        // Alternative check using the helper method
        bool isCompatible = walletIdentifier.IsAssetTypeCompatibleWithAssetPool();
        Console.WriteLine($"Compatibility Check: {(isCompatible ? "✅ Compatible" : "❌ Incompatible")}");
    }
    
    /// <summary>
    /// Example of invalid WalletIdentifier creation - AssetType doesn't match AssetPool's AssetGroup
    /// </summary>
    public static void InvalidExample()
    {
        // Create an AssetPool for FiatAssets
        var fiatAssetPool = new AssetPool
        {
            Id = Guid.NewGuid(),
            BaseAssetHolderId = Guid.NewGuid(),
            AssetGroup = AssetGroup.FiatAssets
        };
        
        // Try to create a WalletIdentifier with Bitcoin AssetType (which belongs to CryptoAssets group)
        var walletIdentifier = new WalletIdentifier
        {
            Id = Guid.NewGuid(),
            AssetPoolId = fiatAssetPool.Id,
            AssetPool = fiatAssetPool,
            AssetType = AssetType.Bitcoin, // ❌ Invalid: Bitcoin belongs to CryptoAssets, not FiatAssets
            AccountClassification = AccountClassification.ASSET
        };
        
        // Validate compatibility
        var validator = new WalletIdentifierValidationService();
        var result = validator.ValidateWalletIdentifier(walletIdentifier);
        
        Console.WriteLine($"Validation Result: {(result.IsValid ? "✅ VALID" : "❌ INVALID")}");
        
        if (!result.IsValid)
        {
            Console.WriteLine("Validation Errors:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Field}: {error.Message}");
            }
        }
        
        // Alternative check using the helper method
        bool isCompatible = walletIdentifier.IsAssetTypeCompatibleWithAssetPool();
        Console.WriteLine($"Compatibility Check: {(isCompatible ? "✅ Compatible" : "❌ Incompatible")}");
    }
    
    /// <summary>
    /// Example showing how to get the expected AssetGroup for any AssetType
    /// </summary>
    public static void AssetTypeToAssetGroupMapping()
    {
        Console.WriteLine("AssetType to AssetGroup Mapping:");
        Console.WriteLine("================================");
        
        var assetTypes = Enum.GetValues<AssetType>();
        
        foreach (var assetType in assetTypes)
        {
            var expectedGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(assetType);
            Console.WriteLine($"{assetType,-20} → {expectedGroup}");
        }
    }
    
    /// <summary>
    /// Example showing validation during WalletIdentifier service creation
    /// </summary>
    public static async Task ServiceValidationExample()
    {
        // This example shows how the validation works in the WalletIdentifierService
        // When you try to create a WalletIdentifier, the service will:
        
        // 1. Determine the expected AssetGroup from the AssetType
        var assetType = AssetType.PokerStars;
        var expectedGroup = WalletIdentifierValidationService.GetAssetGroupForAssetType(assetType);
        Console.WriteLine($"AssetType: {assetType} → Expected AssetGroup: {expectedGroup}");
        
        // 2. Find or create an AssetPool with the correct AssetGroup
        Console.WriteLine($"Looking for AssetPool with AssetGroup: {expectedGroup}");
        
        // 3. Validate that the WalletIdentifier's AssetType is compatible with the AssetPool's AssetGroup
        Console.WriteLine("Validating AssetType/AssetGroup compatibility...");
        
        // 4. If validation fails, throw an exception with detailed error message
        Console.WriteLine("✅ Validation ensures data integrity!");
    }
} 