using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class OfxTransactionResponse
{
    public DateTime Date { get; set; }
    
    public decimal Value { get; set; }

    public string? Description { get; set; }
    
    public Guid BankId { get; set; }

    // public TransactionDirection TransactionDirection { get; set; }
    
    public string FitId { get; set; }

    public Guid OfxId { get; set; }
    
    public Guid? BankTransactionId { get; set; }
}