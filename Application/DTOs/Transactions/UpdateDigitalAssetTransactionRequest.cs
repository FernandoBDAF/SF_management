namespace SFManagement.Application.DTOs.Transactions;

public class UpdateDigitalAssetTransactionRequest
{
    public DateTime? Date { get; set; }

    public decimal? AssetAmount { get; set; }

    public decimal? ConversionRate { get; set; }

    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }
}
