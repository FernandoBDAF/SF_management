namespace SFManagement.Models
{
    public class Manager : BaseDomain
    {
        public string? Name { get; set; }

        public virtual List<Wallet> Wallets { get; set; } = new List<Wallet>();
    }
}
