using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class WalletTransactionResponse : BaseResponse
    {
        public decimal Value { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public WalletTransactionType WalletTransactionType { get; set; }

        public Guid WalletId { get; set; }
    }
}
