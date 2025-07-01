using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class BaseTransactionRequest
{
    public DateTime? Date { get; set; }
    
    public Guid? WalletIdentifierId { get; set; }
    
    public Guid?  AssetWalletId { get; set; }

    public string? Description { get; set; }
    
    public decimal? AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }
    
    public Guid? FinancialBehaviorId { get; set; }
}