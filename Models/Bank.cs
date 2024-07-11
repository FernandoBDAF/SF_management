namespace SFManagement.Models
{
    public class Bank : BaseDomain
    {
        public string? Code { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();
    }
}
