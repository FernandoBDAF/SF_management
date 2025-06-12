using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class BankTransaction : BaseTransaction
{
    [Required] [ForeignKey("Bank")] public Guid BankId { get; set; }
    
    // Banks are entities that manager Fiat transactions
    [Required][Precision(18, 2)] public decimal FiatAmount { get; set; }

    [Required] public FiatCurrencyType FiatCurrencyType { get; set; }
    
    [Required] public BankTransactionType BankTransactionType { get; set; }

    [ForeignKey("OfxTransaction")] public Guid? OfxTransactionId { get; set; }

    public virtual OfxTransaction? OfxTransaction { get; set; }
}