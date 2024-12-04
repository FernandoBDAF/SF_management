using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class BankTransactionResponse : BaseResponse
    {
        public Guid BankId { get; set; }

        public decimal Value { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public string? FitId { get; set; }

        public Guid? OfxId { get; set; }

        public Guid? ClientId { get; set; }
		
        public DateTime? ApprovedAt { get; set; }
        
        public string? TagDescription { get; set; }
        
        public Guid? LinkedToId { get; set; }

		public BankTransactionType BankTransactionType { get; set; }

        public bool IsValid { get; set; }

        public string BankName { get; set; }

        public string ClientName { get; set; }

        public Guid? TagId { get; set; }
    }
}
