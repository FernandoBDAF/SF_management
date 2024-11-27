using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models;

namespace SFManagement.ViewModels
{
    public class TransactionResponse
    {
        public TransactionResponse() { }

        public TransactionResponse(BankTransaction bankTransaction)
        {
            Id = bankTransaction.Id;
            Date = bankTransaction.Date;
            Description = bankTransaction.Description;
            Type = bankTransaction.ToString();
            Value = bankTransaction.Value;
            BankTransactionType = bankTransaction.BankTransactionType;
            BankId = bankTransaction.BankId;
            Description = bankTransaction.Description;
        }
        
        public TransactionResponse(WalletTransaction walletTransaction)
        {
            Id = walletTransaction.Id;
            Date = walletTransaction.Date;
            Description = walletTransaction.Description;
            Type = walletTransaction.ToString();
            Value = walletTransaction.Value;
            WalletTransactionType = walletTransaction.WalletTransactionType;
            Coins = walletTransaction.Coins;
            ExchangeRate = walletTransaction.ExchangeRate;
        }

        public Guid Id { get; set; }

        public string? Type { get; set; }

        public string? Description { get; set; }

        [Precision(18, 2)]
        public decimal Value { get; set; }

        public DateTime Date { get; set; }
        
        public BankTransactionType BankTransactionType { get; set; }
        
        public WalletTransactionType WalletTransactionType { get; set; }
        
        public Guid BankId { get; set; }
        
        public decimal ExchangeRate { get; set; }
        
        public decimal Coins { get; set; }

    }
}
