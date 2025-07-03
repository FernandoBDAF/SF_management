using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class WalletIdentifierRequest
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
    
    public Guid? BaseAssetHolderId { get; set; }
}