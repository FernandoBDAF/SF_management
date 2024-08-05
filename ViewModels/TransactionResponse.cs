using Microsoft.EntityFrameworkCore;
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
        }

        public Guid Id { get; set; }

        public string? Type { get; set; }

        public string? Description { get; set; }

        [Precision(18, 2)]
        public decimal Value { get; set; }

        public DateTime Date { get; set; }

    }
}
