using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class OfxTransaction : BaseDomain
{
    public OfxTransaction()
    {
    }
    
    public OfxTransaction(XElement el, Guid bankId)
    {
        // BankId = bankId;
    
        var dtposted = el.Element("DTPOSTED")?.Value.Replace("</DTPOSTED>", string.Empty);
        if (!string.IsNullOrEmpty(dtposted) && DateTime.TryParseExact(dtposted, "yyyyMMdd",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) Date = date;
    
        var trnamt = el.Element("TRNAMT")?.Value.Replace("</TRNAMT>", string.Empty);
        if (!string.IsNullOrEmpty(trnamt) && decimal.TryParse(trnamt,
                NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture, out var value))
        {
            Value = value;
            TransactionDirection = TransactionDirection =
                Value > decimal.Zero ? TransactionDirection.Income : TransactionDirection.Expense;
            Value = Value > decimal.Zero ? Value : decimal.Negate(Value);
        }
    
        Description = el.Element("MEMO")?.Value.Replace("</MEMO>", string.Empty);
        FitId = el.Element("FITID")?.Value.Replace("</FITID>", string.Empty) ?? "not found";
        // BankId = bankId;
        // CreatedAt = DateTime.Now;
    }
    
    [Required] public DateTime Date { get; set; }
    
    [Required] [Precision(18, 2)] public decimal Value { get; set; }
    
    [MaxLength(40)] public string? Description { get; set; }
    
    [Required] public TransactionDirection TransactionDirection { get; set; }

    [Required] [MaxLength(40)] public string FitId { get; set; }
    
    public Guid OfxId { get; set; }
    public virtual Ofx Ofx { get; set; } = new Ofx();
}