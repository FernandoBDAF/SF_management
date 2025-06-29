using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class FiatAssetTransactionResponse : BaseTransactionResponse
{
    public decimal? AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }

    public Guid? OfxTransactionId { get; set; }
}