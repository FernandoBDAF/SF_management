namespace SFManagement.ViewModels;

public class WalletIdentifierResponse : BaseResponse
{
    public string? Nickname { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Pix { get; set; }
    
    public string? InputForTransactions { get; set; } = string.Empty;

    public decimal? DefaultRakeCommission { get; set; }

    public decimal? DefaultParentCommission { get; set; }
    
    public Guid? AssetWalletId { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? MemberId { get; set; }
    
    public Guid? BankId { get; set; }
    
    public Guid? PokerManagerId { get; set; }
}