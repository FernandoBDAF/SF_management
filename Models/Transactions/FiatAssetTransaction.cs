using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models.Transactions;

public class FiatAssetTransaction : BaseTransaction
{
    public Guid? OfxTransactionId { get; set; }
    public virtual OfxTransaction? OfxTransaction { get; set; }
}