using SFManagement.Domain.Enums;

namespace SFManagement.Application.DTOs.Finance;

/// <summary>
/// Summary of company profit from all sources.
/// All values in BRL.
/// </summary>
public class ProfitSummary
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid? ManagerId { get; set; }
    
    /// <summary>
    /// Profit from categorized system operations.
    /// Direction determined by System Wallet position.
    /// </summary>
    public decimal DirectIncome { get; set; }
    
    /// <summary>
    /// Profit from SettlementTransactions for RakeOverrideCommission managers.
    /// Formula: RakeAmount × ((RakeCommission - RakeBack) / 100) × AvgRate
    /// </summary>
    public decimal RakeCommission { get; set; }
    
    /// <summary>
    /// Profit from transactions with Rate field.
    /// Formula: AssetAmount × (Rate / (100 + Rate)) × AvgRate
    /// </summary>
    public decimal RateFees { get; set; }
    
    /// <summary>
    /// Profit for Spread managers from SALE transactions.
    /// Formula: SaleAmount × (SaleRate - AvgRate)
    /// Already in BRL, no conversion needed.
    /// </summary>
    public decimal SpreadProfit { get; set; }
    
    public decimal TotalProfit => DirectIncome + RakeCommission + RateFees + SpreadProfit;
}

public class ProfitByManager
{
    public Guid ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public ManagerProfitType? ManagerProfitType { get; set; }
    public decimal DirectIncome { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal RateFees { get; set; }
    public decimal SpreadProfit { get; set; }
    public decimal TotalProfit { get; set; }
}

public class ProfitBySource
{
    public string Source { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
