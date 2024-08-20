using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class BankTransactionRequest
    {
        public Guid BankId { get; set; }

        public decimal Value { get; set; }

        public string? Description { get; set; }
       
        public DateTime Date { get; set; }

        public string? FitId { get; set; }

        public Guid? ClientId { get; set; }

        public BankTransactionType BankTransactionType { get; set; }

        public BankTransactionTag? Tag { get; set; }

        public string? TagDescription { get; set; }
    }
}
