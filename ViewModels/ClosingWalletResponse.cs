namespace SFManagement.ViewModels
{
    public class ClosingWalletResponse : BaseResponse
    {
        public decimal ReturnRake { get; set; }

        public Guid WalletId { get; set; }

        public WalletResponse Wallet { get; set; }

        public Guid ClosingManagerId { get; set; }
    }
}
