using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models
{
    public class WalletTransaction : BaseDomain
    {
        [Precision(18, 2)]
        public decimal Value { get; set; }

        [Precision(18, 2)]
        public decimal ExchangeRate { get; set; }

        [Precision(18, 2)]
        public decimal? Rate { get; set; }
        
        public bool? IsCoinBalance { get; set; }
        
        [Precision(18, 2)]
        public decimal Coins { get; set; }

        [Precision(18, 2)]
        public decimal Profit { get; set; }
        

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

        // se ja temos walletId e wallet obrigatoriamente eh ligado a manager, precisamos disso?
        [ForeignKey("Manager")]
        public Guid? ManagerId { get; set; }

        public virtual Manager Manager { get; set; }

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

        public string? ExcelNickname { get; set; }

        public Guid? ApprovedBy { get; set; }

        public bool IsValid
        {
            get
            {
                return ApprovedAt.HasValue && NicknameId.HasValue;
            }
        }
    }
}
