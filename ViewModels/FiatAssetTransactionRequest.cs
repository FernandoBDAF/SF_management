using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class FiatAssetTransactionRequest : BaseTransactionRequest
{
    public decimal? AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }

    public Guid? OfxTransactionId { get; set; }
}