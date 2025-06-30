using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.ViewModels;

public class WalletIdentifierResponse : BaseResponse
{
    // Nickname, Routing Number, Agencia
    public string? RouteInfo { get; set; }
    
    // Account Number, email, Conta
    public string? IdentifierInfo { get; set; }
    
    // Account Type, pix, poupanca
    public string? DescriptiveInfo { get; set; }
    
    // Name, 
    public string? ExtraInfo { get; set; }
    
    // PIX, PokerManager input, etc...
    public string? InputForTransactions { get; set; } = string.Empty;
    
    public AssetType? AssetType { get; set; }
    
    public decimal? DefaultRakeCommission { get; set; }

    public decimal? DefaultParentCommission { get; set; }
    
    public Guid? BankId { get; set; }
    public string? BankName { get; set; }
    
    public Guid? ClientId { get; set; }
    public string? ClientName { get; set; }

    public Guid? MemberId { get; set; }
    public string? MemberName { get; set; }

    public Guid? PokerManagerId { get; set; }
    public string? PokerManagerName { get; set; }
    
    // public virtual ICollection<FiatAssetTransaction>? FiatAssetTransactions { get; set; }
    //
    // public virtual ICollection<DigitalAssetTransaction>? DigitalAssetTransactions { get; set; }
}