using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace SFManagement.Models
{
    public class Client : BaseDomain
    {
        public string? Name { get; set; }

        public string? Phone { get; set; }

        public string? CPF { get; set; }

        public string? Cep { get; set; }

        public string? Address { get; set; }

        public string? District { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? Complement { get; set; }

        public string? AddressNumber { get; set; }

        public string? Email { get; set; }

        public DateTime? Birthday { get; set; }

        public virtual List<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
    }
}
