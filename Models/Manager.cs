namespace SFManagement.Models
{
    public class Manager : BaseDomain
    {
        public string? Name { get; set; }

        public virtual List<Wallet> Wallets { get; set; } = new List<Wallet>();

        public virtual List<Excel> Excels { get; set; } = new List<Excel>();

        public virtual List<ClosingManager> ClosingManagers { get; set; } = new List<ClosingManager>();

        public virtual List<BankTransaction> BankTransactions { get; set; } = new List<BankTransaction>();

        public virtual List<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
