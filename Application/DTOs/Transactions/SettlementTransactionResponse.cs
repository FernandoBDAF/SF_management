using SFManagement.Application.DTOs.Support;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Common;

namespace SFManagement.Application.DTOs.Transactions;

public class SettlementTransactionResponse : BaseTransactionResponse
{
    /// <summary>
    /// Rake amount for the settlement
    /// </summary>
    public decimal RakeAmount { get; set; }
    
    /// <summary>
    /// Commission on the rake
    /// </summary>
    public decimal RakeCommission { get; set; }
    
    /// <summary>
    /// Rake back amount (if applicable)
    /// </summary>
    public decimal? RakeBack { get; set; }
    
    /// <summary>
    /// Net settlement amount after rake and commissions
    /// </summary>
    public decimal NetSettlementAmount => AssetAmount - RakeAmount - RakeCommission + (RakeBack ?? 0);
    
    /// <summary>
    /// Effective commission rate
    /// </summary>
    public decimal? EffectiveCommissionRate => AssetAmount > 0 ? (RakeCommission / AssetAmount) * 100 : null;
    
    /// <summary>
    /// Settlement details
    /// </summary>
    public SettlementDetails? SettlementInfo { get; set; }
}

/// <summary>
/// Settlement-specific transaction details
/// </summary>
public class SettlementDetails
{
    public string? SettlementPeriod { get; set; }
    public DateTime? PeriodStart { get; set; }
    public DateTime? PeriodEnd { get; set; }
    public int? TransactionCount { get; set; }
    public decimal? TotalVolume { get; set; }
    public string? SettlementType { get; set; }
}

public class SettlementTransactionSimplifiedResponse : BaseResponse
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public virtual CategoryResponse? FinancialBehavior { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    
    public decimal AssetAmount { get; set; }
    public decimal Rake { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
}