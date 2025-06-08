namespace SFManagement.ViewModels;

public class InternalTransactionApproveRequest
{
    public Guid? TagId { get; set; }

    public Guid? ClientId { get; set; }

    public Guid? ManagerId { get; set; }

    public Guid? BankId { get; set; }

    public Guid? WalletId { get; set; }
}