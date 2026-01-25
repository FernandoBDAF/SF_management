using System.ComponentModel.DataAnnotations;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

/// <summary>
/// Request DTO for creating a transfer between any two asset holders.
/// Supports both Fiat and Digital asset transfers.
/// </summary>
public class TransferRequest
{
    // === Required: Participants ===
    
    /// <summary>
    /// The asset holder sending the assets.
    /// </summary>
    [Required(ErrorMessage = "SenderAssetHolderId is required")]
    public Guid SenderAssetHolderId { get; set; }
    
    /// <summary>
    /// The asset holder receiving the assets.
    /// </summary>
    [Required(ErrorMessage = "ReceiverAssetHolderId is required")]
    public Guid ReceiverAssetHolderId { get; set; }
    
    // === Optional: Specific Wallet Selection ===
    
    /// <summary>
    /// Optional: Specific sender wallet to use.
    /// If provided, must belong to SenderAssetHolderId and match AssetType.
    /// </summary>
    public Guid? SenderWalletIdentifierId { get; set; }
    
    /// <summary>
    /// Optional: Specific receiver wallet to use.
    /// If provided, must belong to ReceiverAssetHolderId and match AssetType.
    /// </summary>
    public Guid? ReceiverWalletIdentifierId { get; set; }
    
    // === Required: Asset Specification ===
    
    /// <summary>
    /// The type of asset being transferred.
    /// Determines whether to create Fiat or Digital transaction.
    /// </summary>
    [Required(ErrorMessage = "AssetType is required")]
    public AssetType AssetType { get; set; }
    
    // === Required: Transaction Details ===
    
    /// <summary>
    /// The amount to transfer. Must be greater than 0.
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
    
    /// <summary>
    /// The date of the transaction.
    /// </summary>
    [Required(ErrorMessage = "Date is required")]
    public DateTime Date { get; set; }
    
    // === Optional: Transaction Details ===
    
    /// <summary>
    /// Optional category for the transaction.
    /// </summary>
    public Guid? CategoryId { get; set; }
    
    /// <summary>
    /// Optional description.
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    
    // === Optional: Digital Asset Specific ===
    
    /// <summary>
    /// For digital transactions: record balance as this asset type.
    /// </summary>
    public AssetType? BalanceAs { get; set; }
    
    /// <summary>
    /// For digital transactions: conversion rate.
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "ConversionRate must be non-negative")]
    public decimal? ConversionRate { get; set; }
    
    /// <summary>
    /// For digital transactions: rate/fee percentage.
    /// </summary>
    [Range(0, 100, ErrorMessage = "Rate must be between 0 and 100")]
    public decimal? Rate { get; set; }
    
    // === Options ===
    
    /// <summary>
    /// DEPRECATED: Automatic wallet creation is no longer supported.
    /// This flag is ignored and will cause an error if set to true.
    /// Create wallets explicitly before initiating transfer.
    /// </summary>
    [Obsolete("Automatic wallet creation is no longer supported. This flag will be removed in a future version.")]
    public bool CreateWalletsIfMissing { get; set; } = false;
    
    /// <summary>
    /// If true, auto-approves the transaction.
    /// Default: false
    /// </summary>
    public bool AutoApprove { get; set; } = false;
    
    /// <summary>
    /// If true, validates sender has sufficient balance.
    /// Default: false
    /// </summary>
    public bool ValidateBalance { get; set; } = false;
}

