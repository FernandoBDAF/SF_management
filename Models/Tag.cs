using System.ComponentModel.DataAnnotations.Schema;
using SFManagement.Models.Transactions;

namespace SFManagement.Models;
// Rename to category
// It's a unique definition that redefine the behaviour of the transaction
public class Tag : BaseDomain
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public virtual Tag? Parent { get; set; }

    public virtual List<Tag> Children { get; set; } = new();

    public virtual List<DigitalAssetTransaction> WalletTransactions { get; set; } = new();

    public virtual List<FiatAssetTransaction> BankTransactions { get; set; } = new();

    public virtual List<InternalTransaction> InternalTransactions { get; set; } = new();
}