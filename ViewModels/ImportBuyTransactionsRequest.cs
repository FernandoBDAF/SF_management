namespace SFManagement.ViewModels
{
    public class ImportBuyTransactionsRequest
    {
        public IFormFile File { get; set; }

        public Guid WalletId { get; set; }
    }
}
