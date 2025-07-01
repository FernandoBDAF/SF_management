using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class InternalTransactionRequest
{
    public decimal Value { get; set; }

    public decimal? Coins { get; set; }

    public decimal? ExchangeRate { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? ManagerId { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    // public TransactionDirection InternalTransactionType { get; set; }

    public Guid? FinancialBehaviorId { get; set; }

    public Guid? WalletId { get; set; }

    public Guid? BankId { get; set; }
}