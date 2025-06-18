using System.ComponentModel.DataAnnotations.Schema;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;
// Rename to category
// It's a unique definition that redefine the behaviour of the transaction

// Use BehaviorLabel or FinancialBehavior if:
// These tags influence how your system processes or interprets the entity’s financial actions.
//
// Use Classification if:
// You're aiming for something structured and system-driven (e.g., enums, rules).
public class Tag : BaseDomain
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public virtual Tag? Parent { get; set; }

    public virtual List<Tag> Children { get; set; } = new();

    public virtual List<DigitalAssetTransaction> WalletTransactions { get; set; } = new();

    public virtual List<FiatAssetTransaction> BankTransactions { get; set; } = new();
}