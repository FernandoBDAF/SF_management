using SFManagement.Enums;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.Services;

public class WalletIdentifierValidationService
{
    public ValidationResult ValidateWalletIdentifier(WalletIdentifier walletIdentifier)
    {
        var result = new ValidationResult();
        
        // Validate AssetType/AssetGroup compatibility
        if (!IsAssetTypeCompatibleWithAssetGroup(walletIdentifier.AssetType, walletIdentifier.AssetPool?.AssetGroup))
        {
            result.AddError("AssetType", 
                $"AssetType '{walletIdentifier.AssetType}' is not compatible with AssetPool's AssetGroup '{walletIdentifier.AssetPool?.AssetGroup}'. " +
                $"Expected AssetGroup: '{GetExpectedAssetGroup(walletIdentifier.AssetType)}'");
        }
        
        // Validate metadata based on the AssetPool's AssetGroup, not the wallet's computed AssetGroup
        var assetPoolGroup = walletIdentifier.AssetPool?.AssetGroup;
        if (assetPoolGroup.HasValue)
        {
            if (!ValidateMetadataForAssetGroup(walletIdentifier, assetPoolGroup.Value))
            {
                result.AddError("Metadata", $"Invalid metadata for {assetPoolGroup.Value}");
            }
            
            // Additional validation based on AssetPool's asset group
            switch (assetPoolGroup.Value)
            {
                case AssetGroup.FiatAssets:
                    ValidateBankWalletSpecific(walletIdentifier, result);
                    break;
                case AssetGroup.PokerAssets:
                    ValidatePokerWalletSpecific(walletIdentifier, result);
                    break;
                case AssetGroup.CryptoAssets:
                    ValidateCryptoWalletSpecific(walletIdentifier, result);
                    break;
                case AssetGroup.Internal:
                    ValidateInternalWalletSpecific(walletIdentifier, result);
                    break;
            }
        }
        
        return result;
    }
    
    /// <summary>
    /// Validates that an AssetType is compatible with an AssetGroup
    /// </summary>
    private static bool IsAssetTypeCompatibleWithAssetGroup(AssetType assetType, AssetGroup? assetGroup)
    {
        if (!assetGroup.HasValue)
            return false;
            
        // Internal AssetGroup can accept any AssetType since it represents company-owned wallets
        if (assetGroup.Value == AssetGroup.Internal)
            return true;
            
        var expectedAssetGroup = GetExpectedAssetGroup(assetType);
        return expectedAssetGroup == assetGroup.Value;
    }
    
    /// <summary>
    /// Gets the expected AssetGroup for a given AssetType
    /// </summary>
    private static AssetGroup GetExpectedAssetGroup(AssetType assetType)
    {
        return assetType switch
        {
            // Fiat Assets
            AssetType.BrazilianReal or AssetType.USDollar => AssetGroup.FiatAssets,
            
            // Poker Assets
            AssetType.PokerStars or AssetType.GgPoker or AssetType.YaPoker or 
            AssetType.AmericasCardRoom or AssetType.SupremaPoker or 
            AssetType.AstroPayICash or AssetType.LuxonPoker => AssetGroup.PokerAssets,
            
            // Crypto Assets
            AssetType.Bitcoin or AssetType.Ethereum or AssetType.Litecoin or 
            AssetType.Ripple or AssetType.BitcoinCash or AssetType.Stellar => AssetGroup.CryptoAssets,
            
            // Default to Internal for unknown types
            _ => AssetGroup.Internal
        };
    }
    
    /// <summary>
    /// Public method to validate AssetType/AssetGroup compatibility (for use in other services)
    /// </summary>
    public static bool ValidateAssetTypeCompatibility(AssetType assetType, AssetGroup assetGroup)
    {
        return IsAssetTypeCompatibleWithAssetGroup(assetType, assetGroup);
    }
    
    /// <summary>
    /// Public method to get expected AssetGroup for an AssetType (for use in other services)
    /// </summary>
    public static AssetGroup GetAssetGroupForAssetType(AssetType assetType)
    {
        return GetExpectedAssetGroup(assetType);
    }
    
    /// <summary>
    /// Validates metadata for a specific asset group
    /// </summary>
    private static bool ValidateMetadataForAssetGroup(WalletIdentifier walletIdentifier, AssetGroup assetGroup)
    {
        return walletIdentifier.ValidateMetadataForAssetGroup(assetGroup);
    }
    
    private void ValidateBankWalletSpecific(WalletIdentifier walletIdentifier, ValidationResult result)
    {
        // Add bank-specific validation logic here
        // For example, validate PIX key format, account number format, etc.
        
        // Ensure it's actually a fiat asset type
        if (walletIdentifier.AssetType != AssetType.BrazilianReal && walletIdentifier.AssetType != AssetType.USDollar)
        {
            result.AddError("AssetType", $"AssetType '{walletIdentifier.AssetType}' is not a valid fiat asset type for FiatAssets group");
        }
    }
    
    private void ValidatePokerWalletSpecific(WalletIdentifier walletIdentifier, ValidationResult result)
    {
        // Add poker-specific validation logic here
        // For example, validate player nickname format, etc.
        
        // Ensure it's actually a poker asset type
        var validPokerTypes = new[] { 
            AssetType.PokerStars, AssetType.GgPoker, AssetType.YaPoker, 
            AssetType.AmericasCardRoom, AssetType.SupremaPoker, 
            AssetType.AstroPayICash, AssetType.LuxonPoker 
        };
        
        if (!validPokerTypes.Contains(walletIdentifier.AssetType))
        {
            result.AddError("AssetType", $"AssetType '{walletIdentifier.AssetType}' is not a valid poker asset type for PokerAssets group");
        }
    }
    
    private void ValidateCryptoWalletSpecific(WalletIdentifier walletIdentifier, ValidationResult result)
    {
        // Add crypto-specific validation logic here
        // For example, validate wallet address format, etc.
        
        // Ensure it's actually a crypto asset type
        var validCryptoTypes = new[] { 
            AssetType.Bitcoin, AssetType.Ethereum, AssetType.Litecoin, 
            AssetType.Ripple, AssetType.BitcoinCash, AssetType.Stellar 
        };
        
        if (!validCryptoTypes.Contains(walletIdentifier.AssetType))
        {
            result.AddError("AssetType", $"AssetType '{walletIdentifier.AssetType}' is not a valid crypto asset type for CryptoAssets group");
        }
    }
    
    private void ValidateInternalWalletSpecific(WalletIdentifier walletIdentifier, ValidationResult result)
    {
        // Add internal wallet-specific validation logic here
        // Internal wallets can potentially have any AssetType, but we might want to add specific rules
    }
}

public class ValidationResult
{
    public List<ValidationError> Errors { get; } = new();
    public bool IsValid => !Errors.Any();
    
    public void AddError(string field, string message)
    {
        Errors.Add(new ValidationError(field, message));
    }
}

public record ValidationError(string Field, string Message); 