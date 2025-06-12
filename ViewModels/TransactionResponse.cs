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

    public TransactionResponse(InternalTransaction internalTransaction)
    {
        Id = internalTransaction.Id;
        Date = internalTransaction.Date;
        Description = internalTransaction.Description;
        Type = internalTransaction.ToString();
        Value = internalTransaction.Value;
        InternalTransactionType = internalTransaction.InternalTransactionType;
        TagId = internalTransaction.TagId;
        ClientId = internalTransaction.ClientId;
        ApprovedAt = internalTransaction.ApprovedAt;
    }

    public TransactionResponse(BankTransaction bankTransaction)
    {
        Id = bankTransaction.Id;
        Date = bankTransaction.Date;
        Description = bankTransaction.Description;
        Type = bankTransaction.ToString();
        Value = bankTransaction.Value;
        BankTransactionType = bankTransaction.BankTransactionType;
        BankId = bankTransaction.BankId;
        TagId = bankTransaction.TagId;
        ClientId = bankTransaction.ClientId;
        ApprovedAt = bankTransaction.ApprovedAt;
        OfxId = bankTransaction.OfxId;
    }

    public TransactionResponse(WalletTransaction walletTransaction)
    {
        Id = walletTransaction.Id;
        WalletId = walletTransaction.WalletId;
        NicknameId = walletTransaction.NicknameId;
        Date = walletTransaction.Date;
        Description = walletTransaction.Description;
        Type = walletTransaction.ToString();
        Value = walletTransaction.Value;
        WalletTransactionType = walletTransaction.WalletTransactionType;
        Coins = walletTransaction.Coins;
        ExchangeRate = walletTransaction.ExchangeRate;
        TagId = walletTransaction.TagId;
        ClientId = walletTransaction.ClientId;
        ApprovedAt = walletTransaction.ApprovedAt;
        ExcelId = walletTransaction.ExcelId;
        Profit = walletTransaction.Profit;
    }

    public Guid Id { get; set; }

    public Guid? WalletId { get; set; }

    public Guid? NicknameId { get; set; }

    public string? Type { get; set; }

    public string? Description { get; set; }

    [Precision(18, 2)] public decimal Value { get; set; }

    public DateTime Date { get; set; }

    public BankTransactionType BankTransactionType { get; set; }

    public WalletTransactionType WalletTransactionType { get; set; }

    public InternalTransactionType InternalTransactionType { get; set; }

    public Guid BankId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal Coins { get; set; }

    public Guid? TagId { get; set; }

    public Guid? ClientId { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ExcelId { get; set; }

    public Guid? OfxId { get; set; }

    public decimal Profit { get; set; }
}