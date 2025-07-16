using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class StatementTransactionResponse
{
    public Guid Id { get; set; }
    
    public DateTime Date { get; set; }
    
    public string? Description { get; set; }
    
    public decimal? AssetAmount { get; set; }
    
    public AssetType? BalanceAs { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public decimal? Rate { get; set; }
    
    public AssetType AssetType { get; set; }
    
    public string? CounterPartyName { get; set; }
    
    public string? WalletIdentifierInput { get; set; }

    public AssetGroup AssetGroup { get; set; }
}