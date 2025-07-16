namespace SFManagement.ViewModels;

public class BankTransactionApproveRequest
{
    public Guid? FinancialBehaviorId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? ManagerId { get; set; }
}