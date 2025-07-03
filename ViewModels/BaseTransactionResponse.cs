using SFManagement.Models;
using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class BaseTransactionResponse : BaseResponse
{
    public DateTime Date { get; set; }
    
    public virtual WalletIdentifierResponse? WalletIdentifier { get; set; }
    
    public virtual AssetWalletResponse? AssetWallet { get; set; }

    public string? Description { get; set; }
    
    public virtual FinancialBehaviorResponse? FinancialBehavior { get; set; }
    
    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}