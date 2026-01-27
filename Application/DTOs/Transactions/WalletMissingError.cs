namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Details about missing wallets for transfer validation.
/// </summary>
public class WalletMissingError
{
    public string Code { get; set; } = "WALLETS_REQUIRED";
    public string Message { get; set; } =
        "One or more wallets need to be created to complete this transfer.";

    // Sender details
    public bool SenderWalletMissing { get; set; }
    public Guid? SenderAssetHolderId { get; set; }
    public string? SenderAssetHolderName { get; set; }
    public string? SenderAssetHolderType { get; set; }
    public string? SenderAssetTypeName { get; set; }

    // Receiver details
    public bool ReceiverWalletMissing { get; set; }
    public Guid? ReceiverAssetHolderId { get; set; }
    public string? ReceiverAssetHolderName { get; set; }
    public string? ReceiverAssetHolderType { get; set; }
    public string? ReceiverAssetTypeName { get; set; }
}

