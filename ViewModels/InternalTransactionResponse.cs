using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class InternalTransactionResponse : BaseResponse
    {
        public decimal Value { get; set; }

        public decimal? Coins { get; set; }

        public decimal? ExchangeRate { get; set; }

        public Guid? ClientId { get; set; }

        public Guid? ManagerId { get; set; }

        public InternalTransactionType InternalTransactionType { get; set; }

        public Guid? TransferId { get; set; }

        public DateTime Date { get; set; }

        public string? Description { get; set; }

        public Guid? TagId { get; set; }

        public Guid? WalletId { get; set; }

        public Guid? BankId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public Guid? ApprovedBy { get; set; }
        
        public Guid? ClosingManagerId { get; set; }

    }
}
