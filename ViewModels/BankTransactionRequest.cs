using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class BankTransactionRequest
{
    public Guid BankId { get; set; }

    public decimal Value { get; set; }

    public string? Description { get; set; }

    public DateTime Date { get; set; }

    public string? FitId { get; set; }

    public Guid? ClientId { get; set; }

    public TransactionDirection TransactionDirection { get; set; }

    public Guid? TagId { get; set; }

    public Guid? ManagerId { get; set; }
}