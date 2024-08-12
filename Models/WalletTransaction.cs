using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace SFManagement.Models
{
    public class WalletTransaction : BaseDomain
    {
        [Precision(18, 2)]
        public decimal Value { get; set; }

        public string? Description { get; set; }

        public DateTime Date { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public WalletTransactionType WalletTransactionType { get; set; }

        [ForeignKey("Wallet")]
        public Guid WalletId { get; set; }

        public virtual Wallet Wallet { get; set; }

        public bool IsValid
        {
            get
            {
                return ApprovedAt.HasValue;
            }
        }
    }
}
