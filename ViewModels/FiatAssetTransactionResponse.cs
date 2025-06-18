using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class FiatAssetTransactionResponse : BaseResponse
{
    public decimal? AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }

    public Guid? OfxTransactionId { get; set; }
}