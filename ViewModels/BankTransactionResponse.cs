using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class BankTransactionResponse : BaseResponse
    {
        public Guid BankId { get; set; }

        public decimal Value { get; set; }

        public DateTime Date { get; set; }

        public string? FitId { get; set; }

        public string? Description { get; set; }

        public bool IsValid { get; set; }

        public Guid? ClientId { get; set; }

        public DateTime? ImportedFromOfxFileAt { get; set; }

        public BankTransactionTag? Tag { get; set; }

        public string? TagDescription { get; set; }
    }
}
