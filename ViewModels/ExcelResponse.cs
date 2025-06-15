namespace SFManagement.ViewModels;

public class ExcelResponse : BaseResponse
{
    public Guid ManagerId { get; set; }

    public string? FileName { get; set; }

    public string? FileType { get; set; }

    public ICollection<DigitalAssetTransactionResponse> WalletTransactions { get; set; } =
        new HashSet<DigitalAssetTransactionResponse>();
}