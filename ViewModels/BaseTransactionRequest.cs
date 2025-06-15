using SFManagement.Models;
using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class BaseTransactionRequest
{
    public DateTime? Date { get; set; }
    
    public Guid? WalletIdentifierId { get; set; }
    
    public Guid?  AssetWalletId { get; set; }

    public string? Description { get; set; }
    
    public Guid? TagId { get; set; }
}