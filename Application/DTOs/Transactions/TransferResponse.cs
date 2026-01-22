using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Response DTO for a completed transfer transaction.
/// </summary>
public class TransferResponse
{
    // Transaction identification
    public Guid TransactionId { get; set; }
    public string EntityType { get; set; } = string.Empty; // "fiat" or "digital"
    public AssetType AssetType { get; set; }
    
    // Sender details
    public Guid SenderWalletIdentifierId { get; set; }
    public Guid SenderAssetHolderId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    
    // Receiver details
    public Guid ReceiverWalletIdentifierId { get; set; }
    public Guid ReceiverAssetHolderId { get; set; }
    public string ReceiverName { get; set; } = string.Empty;
    
    // Transaction details
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public bool IsInternalTransfer { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Wallet creation indicators
    public bool SenderWalletCreated { get; set; }
    public bool ReceiverWalletCreated { get; set; }
    public bool WalletsCreated => SenderWalletCreated || ReceiverWalletCreated;
}

