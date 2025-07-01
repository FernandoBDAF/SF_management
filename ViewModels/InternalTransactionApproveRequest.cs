namespace SFManagement.ViewModels;

public class InternalTransactionApproveRequest
{
    public Guid? FinancialBehaviorId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? ManagerId { get; set; }

    public Guid? BankId { get; set; }

    public Guid? WalletId { get; set; }
}