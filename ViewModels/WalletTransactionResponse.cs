using SFManagement.Enums;

namespace SFManagement.ViewModels
{
    public class WalletTransactionResponse : BaseResponse
    {
        public decimal Value { get; set; }
        
        public decimal Coins { get; set; }
        
        public decimal ExchangeRate { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public WalletTransactionType WalletTransactionType { get; set; }

        public Guid? WalletId { get; set; }

        public Guid? NicknameId { get; set; }

        public Guid? ClientId { get; set; }
        
        public Guid? ExcelId { get; set; }
        
        public Guid? LinkedToId { get; set; }

        public Guid? TagId { get; set; }

        public Guid? ManagerId { get; set; }
    }
}
