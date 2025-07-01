using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class TransactionResponse
{
    public TransactionResponse()
    {
    }

    // public TransactionResponse(InternalTransaction internalTransaction)
    // {
    //     Id = internalTransaction.Id;
    //     Date = internalTransaction.Date;
    //     Description = internalTransaction.Description;
    //     Type = internalTransaction.ToString();
    //     // Value = internalTransaction.Value;
    //     InternalTransactionType = internalTransaction.InternalTransactionType;
    //     // TagId = internalTransaction.TagId;
    //     // ClientId = internalTransaction.ClientId;
    //     ApprovedAt = internalTransaction.ApprovedAt;
    // }

    public TransactionResponse(FiatAssetTransaction fiatAssetTransaction)
    {
        // Id = fiatAssetTransaction.Id;
        // Date = fiatAssetTransaction.Date;
        // Description = fiatAssetTransaction.Description;
        // Type = fiatAssetTransaction.ToString();
        // Value = fiatAssetTransaction.Value;
        // BankTransactionType = fiatAssetTransaction.BankTransactionType;
        // BankId = fiatAssetTransaction.BankId;
        // TagId = fiatAssetTransaction.TagId;
        // ClientId = fiatAssetTransaction.ClientId;
        // ApprovedAt = fiatAssetTransaction.ApprovedAt;
        // OfxId = fiatAssetTransaction.OfxId;
    }

    public TransactionResponse(DigitalAssetTransaction digitalAssetTransaction)
    {
        // Id = digitalAssetTransaction.Id;
        // WalletId = digitalAssetTransaction.WalletId;
        // NicknameId = digitalAssetTransaction.NicknameId;
        // Date = digitalAssetTransaction.Date;
        // Description = digitalAssetTransaction.Description;
        // Type = digitalAssetTransaction.ToString();
        // Value = digitalAssetTransaction.Value;
        // WalletTransactionType = digitalAssetTransaction.WalletTransactionType;
        // Coins = digitalAssetTransaction.Coins;
        // ExchangeRate = digitalAssetTransaction.ExchangeRate;
        // TagId = digitalAssetTransaction.TagId;
        // ClientId = digitalAssetTransaction.ClientId;
        // ApprovedAt = digitalAssetTransaction.ApprovedAt;
        // ExcelId = digitalAssetTransaction.ExcelId;
        // Profit = digitalAssetTransaction.Profit;
    }

    public Guid Id { get; set; }

    public Guid? WalletId { get; set; }

    public Guid? NicknameId { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    [Precision(18, 2)] public decimal Value { get; set; }

    public DateTime Date { get; set; }

    public TransactionDirection BankTransactionType { get; set; }

    public TransactionDirection WalletTransactionType { get; set; }

    public TransactionDirection InternalTransactionType { get; set; }

    public Guid BankId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal Coins { get; set; }

    public Guid? FinancialBehaviorId { get; set; }

    public Guid? ClientId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ExcelId { get; set; }

    public Guid? OfxId { get; set; }

    public decimal Profit { get; set; }
}