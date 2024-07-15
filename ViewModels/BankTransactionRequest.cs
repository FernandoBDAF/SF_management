using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class BankTransactionRequest
    {
        public Guid BankId { get; set; }

        public decimal Value { get; set; }

        public string? Description { get; set; }

        public BankTransactionType BankTransactionType { get; set; }
    }
}
