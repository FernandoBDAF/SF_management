namespace SFManagement.ViewModels
{
    public class ImportSellTransactionsRequest
    {
        public IFormFile File { get; set; }

        public Guid WalletId { get; set; }
    }
}
