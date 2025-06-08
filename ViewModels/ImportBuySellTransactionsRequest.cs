namespace SFManagement.ViewModels;

public class ImportBuySellTransactionsRequest
{
    public IFormFile File { get; set; }

    public Guid WalletId { get; set; }
}