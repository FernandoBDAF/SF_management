using SFManagement.Models.Transactions;

namespace SFManagement.Models.Support;
// These tags influence how the system processes or interprets the entity’s financial actions.
public class FinancialBehavior : BaseDomain
{
    public string Description { get; set; }

    public Guid? FinancialBehaviorId { get; set; }
    public virtual FinancialBehavior? Parent { get; set; }

    public virtual List<FinancialBehavior> Children { get; set; } = new();

    public virtual List<DigitalAssetTransaction> WalletTransactions { get; set; } = new();

    public virtual List<FiatAssetTransaction> BankTransactions { get; set; } = new();
}