using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class BaseTransactionRequest
{
    public DateTime Date { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public Guid SenderWalletIdentifierId { get; set; }
    public Guid ReceiverWalletIdentifierId { get; set; }
    
    public decimal AssetAmount { get; set; }

    public string? Description { get; set; }
    
    
}