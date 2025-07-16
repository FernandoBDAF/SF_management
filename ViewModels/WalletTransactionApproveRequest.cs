namespace SFManagement.ViewModels;

public class WalletTransactionApproveRequest
{
    public Guid? FinancialBehaviorId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? NicknameId { get; set; }

    public Guid? WalletId { get; set; }

    public decimal ExchangeRate { get; set; }

    public decimal Value { get; set; }
}