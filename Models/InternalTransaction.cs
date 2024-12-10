
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models
{
    public class InternalTransaction : BaseDomain
    {
        [Precision(18, 2)]
        public decimal Value { get; set; }

        [Precision(18, 2)]
        public decimal? Coins { get; set; }

        [Precision(18, 2)]
        public decimal? ExchangeRate { get; set; }

        [ForeignKey("Client")]
        public Guid? ClientId { get; set; }

        public virtual Client Client { get; set; }

        [ForeignKey("Manager")]
        public Guid? ManagerId { get; set; }

        public virtual Manager Manager { get; set; }

        public InternalTransactionType InternalTransactionType { get; set; }

        public Guid? TransferId { get; set; }
    }
}
