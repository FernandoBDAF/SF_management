using Microsoft.EntityFrameworkCore;

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

        public virtual List<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();

        public virtual List<Nickname> Nicknames { get; set; } = new List<Nickname>();

        [Precision(18, 2)]
        public decimal InitialValue { get; set; }
    }
}
