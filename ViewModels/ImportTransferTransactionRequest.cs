namespace SFManagement.ViewModels
{
    public class ImportTransferTransactionRequest
    {
        public IFormFile File { get; set; }

        public Guid WalletId { get; set; }
    }
}
