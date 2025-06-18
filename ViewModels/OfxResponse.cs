namespace SFManagement.ViewModels;

public class OfxResponse : BaseResponse
{
    public Guid BankId { get; set; }

    public string? FileName { get; set; }

    public List<OfxTransactionResponse>? OfxTransactions { get; set; } = new();
}