namespace SFManagement.Application.DTOs.Finance;

// ──────────────────────────────────────────────
// Rate Fee Details
// ──────────────────────────────────────────────

public class RateFeeDetailsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<RateFeeItem> Items { get; set; } = new();
    public decimal TotalFeeChips { get; set; }
    public decimal TotalFeeBRL { get; set; }
}

public class RateFeeItem
{
    public Guid TransactionId { get; set; }
    public DateTime Date { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public Guid ManagerId { get; set; }

    /// <summary>Original transaction amount (includes fee).</summary>
    public decimal AssetAmount { get; set; }

    /// <summary>Fee percentage on this transaction.</summary>
    public decimal RatePct { get; set; }

    /// <summary>Fee portion in chips: AssetAmount × (Rate / (100 + Rate))</summary>
    public decimal FeeChips { get; set; }

    /// <summary>AvgRate used for BRL conversion.</summary>
    public decimal AvgRate { get; set; }

    /// <summary>Fee in BRL: FeeChips × AvgRate</summary>
    public decimal FeeBRL { get; set; }
}

// ──────────────────────────────────────────────
// Rake Commission Details
// ──────────────────────────────────────────────

public class RakeCommissionDetailsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<RakeCommissionItem> Items { get; set; } = new();
    public decimal TotalRakeChips { get; set; }
    public decimal TotalRakeBRL { get; set; }
}

public class RakeCommissionItem
{
    public Guid SettlementId { get; set; }
    public DateTime Date { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public Guid ManagerId { get; set; }

    /// <summary>Total chips raked from the table.</summary>
    public decimal RakeAmount { get; set; }

    /// <summary>Commission percentage (before rakeback).</summary>
    public decimal RakeCommissionPct { get; set; }

    /// <summary>Rakeback percentage returned to players.</summary>
    public decimal RakeBackPct { get; set; }

    /// <summary>Net commission chips: RakeAmount × ((Commission - RakeBack) / 100)</summary>
    public decimal RakeChips { get; set; }

    /// <summary>AvgRate used for BRL conversion.</summary>
    public decimal AvgRate { get; set; }

    /// <summary>Commission in BRL: RakeChips × AvgRate</summary>
    public decimal RakeBRL { get; set; }
}

// ──────────────────────────────────────────────
// Spread Profit Details
// ──────────────────────────────────────────────

public class SpreadProfitDetailsResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<SpreadProfitItem> Items { get; set; } = new();
    public decimal TotalSpreadBRL { get; set; }
}

public class SpreadProfitItem
{
    public Guid TransactionId { get; set; }
    public DateTime Date { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public Guid ManagerId { get; set; }

    /// <summary>Amount of chips sold.</summary>
    public decimal AssetAmount { get; set; }

    /// <summary>Sale rate (BRL per chip).</summary>
    public decimal SaleRate { get; set; }

    /// <summary>AvgRate (cost basis) at time of sale.</summary>
    public decimal AvgRate { get; set; }

    /// <summary>Profit: AssetAmount × (SaleRate - AvgRate). Already in BRL.</summary>
    public decimal SpreadBRL { get; set; }
}
