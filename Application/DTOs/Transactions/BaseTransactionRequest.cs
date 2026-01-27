using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;

namespace SFManagement.Application.DTOs.Transactions;

public class BaseTransactionRequest
{
    public DateTime Date { get; set; }
    
    public Guid? CategoryId { get; set; }
    
    public Guid SenderWalletIdentifierId { get; set; }
    public Guid ReceiverWalletIdentifierId { get; set; }
    
    public decimal AssetAmount { get; set; }

    public string? Description { get; set; }
    
    
}