using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace SFManagement.Models
{
    public class BankTransaction : BaseDomain
    {
        public BankTransaction() { }

        public BankTransaction(XElement el, Guid bankId)
        {
            BankId = bankId;

            var dtposted = el.Element("DTPOSTED")?.Value.Replace("</DTPOSTED>", string.Empty);
            if (!string.IsNullOrEmpty(dtposted) && DateTime.TryParseExact(dtposted, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                Date = date;
            }

            var trnamt = el.Element("TRNAMT")?.Value.Replace("</TRNAMT>", string.Empty);
            if (!string.IsNullOrEmpty(trnamt) && decimal.TryParse(trnamt, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var value))
            {
                Value = value;
                BankTransactionType = BankTransactionType = Value > decimal.Zero ? BankTransactionType.Income : BankTransactionType.Expense;
                Value = Value > decimal.Zero ? Value : decimal.Negate(Value);
            }

            Description = el.Element("MEMO")?.Value.Replace("</MEMO>", string.Empty);
            FitId = el.Element("FITID")?.Value.Replace("</FITID>", string.Empty);
            BankId = bankId;
            CreatedAt = DateTime.Now;
        }

        [Precision(18, 2)]
        public decimal Value { get; set; }

        public string? Description { get; set; }

        [ForeignKey("Bank")]
        public Guid BankId { get; set; }

        public virtual Bank Bank { get; set; }

        public BankTransactionType BankTransactionType { get; set; }

        public DateTime Date { get; set; }

        public string? FitId { get; set; }

        [ForeignKey("Ofx")]
        public Guid? OfxId { get; set; }

        public virtual Ofx? Ofx { get; set; }

        [ForeignKey("Client")]
        public Guid? ClientId { get; set; }

        public virtual Client? Client { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public BankTransactionTag? Tag { get; set; }

        public string? TagDescription { get; set; }

        [ForeignKey("LinkedTo")]
        public Guid? LinkedToId { get; set; }

        public virtual BankTransaction LinkedTo { get; set; }

        public virtual BankTransaction WasLinked { get; set; }

        public bool IsValid
        {
            get
            {
                return (!OfxId.HasValue || (OfxId.HasValue && ApprovedAt.HasValue));
            }
        }
    }
}
