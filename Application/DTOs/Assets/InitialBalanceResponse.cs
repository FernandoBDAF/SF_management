using SFManagement.Application.DTOs.Common;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Assets;

/// <summary>
/// Response model for initial balance operations
/// </summary>
public class InitialBalanceResponse : BaseResponse
{
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string AssetTypeName { get; set; } = string.Empty;
    public AssetGroup AssetGroup { get; set; }
    public string AssetGroupName { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public AssetType? BalanceAs { get; set; }
    public string? BalanceAsName { get; set; }
    public decimal? ConversionRate { get; set; }
    public decimal EffectiveBalance { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsAssetTypeBalance => AssetType != 0;
    public bool IsAssetGroupBalance => AssetGroup != 0;
}

/// <summary>
/// Summary response for BaseAssetHolder balances including initial balances
/// </summary>
public class AssetHolderBalanceSummaryResponse
{
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public List<AssetTypeBalanceResponse> AssetTypeBalances { get; set; } = new();
    public List<AssetGroupBalanceResponse> AssetGroupBalances { get; set; } = new();
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
    public bool HasInitialBalance { get; set; }
    public string? InitialBalanceDescription { get; set; }
}

/// <summary>
/// Balance information for a specific AssetGroup
/// </summary>
public class AssetGroupBalanceResponse
{
    public AssetGroup AssetGroup { get; set; }
    public string AssetGroupName { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
    public bool HasInitialBalance { get; set; }
    public string? InitialBalanceDescription { get; set; }
}