using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models
{
    public class Wallet : BaseDomain
    {
        [Precision(18, 2)]
        public decimal IntialCredits { get; set; }

        [Precision(18, 2)]
        public decimal IntialBalance { get; set; }

        [Precision(18, 2)]
        public decimal InitialRate { get; set; }

        [ForeignKey("Manager")]
        public Guid ManagerId { get; set; }

        public string? Name { get; set; }

        public virtual Manager Manager { get; set; }

        public virtual List<Nickname> Nicknames { get; set; } = new List<Nickname>();
    }
}
