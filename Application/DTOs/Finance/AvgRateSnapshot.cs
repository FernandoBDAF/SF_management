namespace SFManagement.Application.DTOs.Finance;

/// <summary>
/// Represents the AvgRate state at the end of a month.
/// Used for caching and profit calculations.
/// </summary>
public class AvgRateSnapshotResponse
{
    public Guid PokerManagerId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    
    /// <summary>
    /// Weighted average cost per chip (BRL/chip) at end of month.
    /// </summary>
    public decimal AvgRate { get; set; }
    
    /// <summary>
    /// Total chips held at end of month.
    /// </summary>
    public decimal TotalChips { get; set; }
    
    /// <summary>
    /// Total cost basis in BRL at end of month.
    /// TotalCost / TotalChips = AvgRate
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// When this snapshot was calculated.
    /// </summary>
    public DateTime CalculatedAt { get; set; }
}
