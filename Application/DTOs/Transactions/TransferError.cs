namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Error details for transfer operations.
/// </summary>
public class TransferError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Error codes for transfer operations.
/// </summary>
public static class TransferErrorCodes
{
    public const string SenderNotFound = "SENDER_NOT_FOUND";
    public const string ReceiverNotFound = "RECEIVER_NOT_FOUND";
    public const string SenderWalletNotFound = "SENDER_WALLET_NOT_FOUND";
    public const string ReceiverWalletNotFound = "RECEIVER_WALLET_NOT_FOUND";
    public const string WalletCreationFailed = "WALLET_CREATION_FAILED";
    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string InvalidAssetType = "INVALID_ASSET_TYPE";
    public const string AssetTypeMismatch = "ASSET_TYPE_MISMATCH";
    public const string WalletOwnershipMismatch = "WALLET_OWNERSHIP_MISMATCH";
    public const string InvalidAmount = "INVALID_AMOUNT";
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string TransactionFailed = "TRANSACTION_FAILED";
    public const string SameSenderReceiverWallet = "SAME_SENDER_RECEIVER_WALLET";
}

