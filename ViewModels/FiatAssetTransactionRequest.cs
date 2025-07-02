using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class FiatAssetTransactionRequest : BaseTransactionRequest
{
    public Guid? OfxTransactionId { get; set; }
    
    public Guid? BaseAssetHolderId { get; set; }
}