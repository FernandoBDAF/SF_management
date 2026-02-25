using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

public class StatementTransactionResponse
{
    public Guid Id { get; set; }
    
    public DateTime Date { get; set; }
    
    public string? Description { get; set; }
    
    public decimal? AssetAmount { get; set; }
    
    public AssetType? BalanceAs { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public decimal? Rate { get; set; }

    /// <summary>
    /// Embedded rate fee amount in chips for digital transactions without BalanceAs.
    /// This keeps statement amount raw while exposing the fee portion to users.
    /// </summary>
    public decimal? RateFeeAmount { get; set; }
    
    public AssetType AssetType { get; set; }
    
    public string? CounterPartyName { get; set; }
    
    public string? WalletIdentifierInput { get; set; }

    public AssetGroup AssetGroup { get; set; }

    public decimal? RakeAmount { get; set; }

    public decimal? RakeCommission { get; set; }

    public decimal? RakeBack { get; set; }

    public decimal? RakeBackAmount { get; set; }
}