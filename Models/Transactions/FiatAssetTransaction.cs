using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

// Banks are entities that manager Fiat transactions
public class FiatAssetTransaction : BaseTransaction
{
    [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    [Required] public TransactionDirection TransactionDirection { get; set; }

    public Guid? OfxTransactionId { get; set; }
    public virtual OfxTransaction? OfxTransaction { get; set; }
}