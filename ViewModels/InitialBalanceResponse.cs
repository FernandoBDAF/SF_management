using SFManagement.Enums;

namespace SFManagement.ViewModels;

/// <summary>
/// Response model for initial balance operations
/// </summary>
public class InitialBalanceResponse : BaseResponse
{
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public AssetType BalanceUnit { get; set; }
    public string BalanceUnitName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AssetType? BalanceAs { get; set; }
    public string? BalanceAsName { get; set; }
    public decimal? ConversionRate { get; set; }
    public decimal EffectiveBalance { get; set; }
    public AssetType EffectiveAssetType { get; set; }
    public string EffectiveAssetTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Summary response for BaseAssetHolder balances including initial balances
/// </summary>
public class AssetHolderBalanceSummaryResponse
{
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public List<AssetTypeBalanceResponse> Balances { get; set; } = new();
    public decimal TotalBalanceInBaseCurrency { get; set; }
    public AssetType BaseCurrency { get; set; } = AssetType.BrazilianReal;
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Balance information for a specific AssetType
/// </summary>
public class AssetTypeBalanceResponse
{
    public AssetType AssetType { get; set; }
    public string AssetTypeName { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public decimal TransactionBalance { get; set; }
    public decimal TotalBalance { get; set; }
    public bool HasInitialBalance { get; set; }
    public string? InitialBalanceDescription { get; set; }
}