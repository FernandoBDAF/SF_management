
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models
{
    public class InternalTransaction : BaseDomain
    {
        [Precision(18, 2)]
        public decimal Value { get; set; }

        [ForeignKey("Client")]
        public Guid ClientId { get; set; }

        public virtual Client Client { get; set; }

        public InternalTransactionType InternalTransactionType { get; set; }
    }
}
