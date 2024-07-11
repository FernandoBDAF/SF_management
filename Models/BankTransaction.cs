using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class BankTransaction : BaseDomain
    {
        [Precision(18, 2)]
        public decimal Value { get; set; }

        public string? Description { get; set; }

        [ForeignKey("Bank")]
        public Guid BankId { get; set; }

        public virtual required Bank Bank { get; set; }

        public BankTransactionType BankTransactionType { get; set; }
    }
}
