using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class FiatAssetTransactionRequest : BaseTransactionRequest
{
    public Guid? OfxTransactionId { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? BankId { get; set; }
}