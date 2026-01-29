namespace SFManagement.Application.DTOs.Finance;

public class DirectIncomeDetailsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<DirectIncomeItem> Incomes { get; set; } = new();
    public List<DirectIncomeItem> Expenses { get; set; } = new();
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetDirectIncome { get; set; }
}

public class DirectIncomeItem
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Origin { get; set; } = string.Empty;
    public DirectIncomeTransactionType TransactionType { get; set; }
    public Guid? CategoryId { get; set; }
}

public enum DirectIncomeTransactionType
{
    FiatAsset,
    DigitalAsset
}
