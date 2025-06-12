using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class BankTransaction : BaseTransaction
{
    public BankTransaction(Guid bankId, BankTransactionType transactionType, DateTime date, decimal value)
    {
        Date = date;
        Value = value;
        BankId = bankId;
        BankTransactionType = transactionType;
    }
    // public BankTransaction()
    // {
    // }

    // public BankTransaction(XElement el, Guid bankId)
    // {
    //     BankId = bankId;
    //
    //     var dtposted = el.Element("DTPOSTED")?.Value.Replace("</DTPOSTED>", string.Empty);
    //     if (!string.IsNullOrEmpty(dtposted) && DateTime.TryParseExact(dtposted, "yyyyMMdd",
    //             CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) Date = date;
    //
    //     var trnamt = el.Element("TRNAMT")?.Value.Replace("</TRNAMT>", string.Empty);
    //     if (!string.IsNullOrEmpty(trnamt) && decimal.TryParse(trnamt,
    //             NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign,
    //             CultureInfo.InvariantCulture, out var value))
    //     {
    //         Value = value;
    //         BankTransactionType = BankTransactionType =
    //             Value > decimal.Zero ? BankTransactionType.Income : BankTransactionType.Expense;
    //         Value = Value > decimal.Zero ? Value : decimal.Negate(Value);
    //     }
    //
    //     Description = el.Element("MEMO")?.Value.Replace("</MEMO>", string.Empty);
    //     FitId = el.Element("FITID")?.Value.Replace("</FITID>", string.Empty);
    //     BankId = bankId;
    //     CreatedAt = DateTime.Now;
    // }

    [Required]
    [ForeignKey("Bank")] public Guid BankId { get; set; }

    public virtual Bank Bank { get; set; } = new();
    
    [Required]
    public BankTransactionType BankTransactionType { get; set; }

    [ForeignKey("OfxTransaction")] public Guid? OfxTransactionId { get; set; }

    public virtual OfxTransaction? OfxTransaction { get; set; } = new();

    // [ForeignKey("LinkedTo")] public Guid? LinkedToId { get; set; }
    //
    // public virtual BankTransaction LinkedTo { get; set; }
    //
    // public virtual BankTransaction WasLinked { get; set; }

    // public bool IsValid => !OfxId.HasValue || (OfxId.HasValue && ApprovedAt.HasValue);
}