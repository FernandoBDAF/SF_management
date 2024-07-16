using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml;

namespace SFManagement.Models
{
    public class BankTransaction : BaseDomain
    {
        public BankTransaction() { }

        public BankTransaction(XmlReader reader, Guid bankId)
        {
            BankId = bankId;

            var dtposted = reader.GetAttribute("DTPOSTED");
            if (!string.IsNullOrEmpty(dtposted) && DateTime.TryParseExact(dtposted, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                Date = date;
            }

            var trnamt = reader.GetAttribute("TRNAMT");
            if (!string.IsNullOrEmpty(trnamt) && decimal.TryParse(trnamt, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
            {
                Value = value;
                BankTransactionType = BankTransactionType = Value > decimal.Zero ? BankTransactionType.Income : BankTransactionType.Expense;
            }

            Description = reader.GetAttribute("MEMO");
            BankId = bankId;

            //TODO: Colocar operação que não tenha cliente vinculado aparece vinculada para empresa (RECEITA ou DESPESA)
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
        public int? ClientId { get; set; }

        public virtual Client? Client { get; set; }

        public DateTime? ImportedFromOfxFileAt { get; set; }

        public BankTransactionTag? Tag { get; set; }

        public string? TagDescription { get; set; }

        public bool IsValid
        {
            get
            {
                return (!OfxId.HasValue || (OfxId.HasValue && ImportedFromOfxFileAt.HasValue));
            }
        }
    }
}
