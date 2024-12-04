using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace SFManagement.Models
{
    public class WalletTransaction : BaseDomain
    {
        [Precision(18, 2)]
        public decimal Value { get; set; }

        [Precision(18, 2)]
        public decimal ExchangeRate { get; set; }

        [Precision(18, 2)]
        public decimal Coins { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public WalletTransactionType WalletTransactionType { get; set; }

        [ForeignKey("Wallet")]
        public Guid? WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        [ForeignKey("Nickname")]
        public Guid? NicknameId { get; set; }

        public virtual Nickname Nickname { get; set; }

        [ForeignKey("Client")]
        public Guid? ClientId { get; set; }

        public virtual Client Client { get; set; }

        [ForeignKey("Excel")]
        public Guid? ExcelId { get; set; }

        public virtual Excel Excel { get; set; }

        [ForeignKey("LinkedTo")]
        public Guid? LinkedToId { get; set; }

        public virtual WalletTransaction LinkedTo { get; set; }

        public virtual WalletTransaction WasLinked { get; set; }

        [ForeignKey("Tag")]
        public Guid? TagId { get; set; }

        public virtual Tag Tag { get; set; }

        public bool IsValid
        {
            get
            {
                return ApprovedAt.HasValue && NicknameId.HasValue;
            }
        }
    }
}
