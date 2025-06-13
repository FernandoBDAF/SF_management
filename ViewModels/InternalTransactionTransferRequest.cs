using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class InternalTransactionTransferRequest
{
    public decimal Value { get; set; }

    public decimal? Coins { get; set; }

    public decimal? ExchangeRate { get; set; }

    public DateTime Date { get; set; }

    public string? Description { get; set; }

    public TransactionDirection InternalTransactionType { get; set; }
}