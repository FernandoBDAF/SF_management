using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class ExcelTransactionResponse
{
    public DateTime Date { get; set; }

    public decimal Coins { get; set; }
    
    public string? Description { get; set; }

    public Guid ManagerId { get; set; }

    public WalletTransactionType WalletTransactionType { get; set; }

    public string ExcelNickname { get; set; }
    
    public string ExcelWallet { get; set; }

    public Guid ExcelId { get; set; }

    public Guid? WalletTransactionId { get; set; }
}