namespace SFManagement.ViewModels
{
    public class ClosingWalletRequest
    {
        public decimal ReturnRake { get; set; }

        public Guid WalletId { get; set; }

        public Guid ClosingManagerId { get; set; }
    }
}
