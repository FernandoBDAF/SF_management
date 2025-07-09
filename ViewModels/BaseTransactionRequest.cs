using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class BaseTransactionRequest
{
    public DateTime? Date { get; set; }
    
    // Sender and Receiver wallet identifiers (new model)
    public Guid? SenderWalletIdentifierId { get; set; }
    public Guid? ReceiverWalletIdentifierId { get; set; }

    public string? Description { get; set; }
    
    public decimal? AssetAmount { get; set; }
    
    public Guid? CategoryId { get; set; }
}