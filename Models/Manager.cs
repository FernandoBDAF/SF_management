namespace SFManagement.Models
{
    public class Manager : BaseDomain
    {
        public string? Name { get; set; }

        public virtual List<Wallet> Wallets { get; set; } = new List<Wallet>();

        public virtual List<Excel> Excels { get; set; } = new List<Excel>();

        public virtual List<ClosingManager> ClosingManagers { get; set; } = new List<ClosingManager>();
    }
}
