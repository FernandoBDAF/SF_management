using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class BaseTransactionResponse : BaseResponse
{
    /// <summary>
    /// Transaction date    
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Transaction amount (always positive)
    /// </summary>
    public decimal AssetAmount { get; set; }
    
    /// <summary>
    /// Transaction description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Approval information
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    
    /// <summary>
    /// Sender wallet information
    /// </summary>
    public WalletIdentifierSummary SenderWallet { get; set; } = new();
    
    /// <summary>
    /// Receiver wallet information
    /// </summary>
    public WalletIdentifierSummary ReceiverWallet { get; set; } = new();
    
    /// <summary>
    /// Transaction category
    /// </summary>
    public CategoryResponse? Category { get; set; }
    
    /// <summary>
    /// Transaction type for client identification
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is an internal transfer (same asset holder)
    /// </summary>
    public bool IsInternalTransfer { get; set; }
    
    /// <summary>
    /// Asset type involved in the transaction
    /// </summary>
    public AssetType AssetType { get; set; }
}

/// <summary>
/// Simplified wallet identifier information for transaction responses
/// </summary>
public class WalletIdentifierSummary
{
    public Guid Id { get; set; }
    public AssetGroup AssetGroup { get; set; }
    public AccountClassification AccountClassification { get; set; }
    public AssetType AssetType { get; set; }
    
    /// <summary>
    /// Asset holder information (null for company pools)
    /// </summary>
    public AssetHolderSummary? AssetHolder { get; set; }
    
    /// <summary>
    /// Key metadata for display purposes (e.g., account number, wallet address)
    /// </summary>
    public string? DisplayMetadata { get; set; }
}

/// <summary>
/// Simplified asset holder information for transaction responses
/// </summary>
public class AssetHolderSummary
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssetHolderType AssetHolderType { get; set; }
    public string? Email { get; set; }
}