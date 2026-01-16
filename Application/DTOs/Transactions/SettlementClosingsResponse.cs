using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;

namespace SFManagement.Application.DTOs.Transactions;

public class SettlementClosingsGroupedResponse
{
    public List<SettlementClosingGroup> ClosingGroups { get; set; } = new();
}

public class SettlementClosingGroup
{
    public DateTime Date { get; set; }
    public List<SettlementTransactionResponse> Transactions { get; set; } = new();
    public int TransactionCount => Transactions.Count;

    public decimal TotalAssetAmount => Transactions.Sum(t => t.AssetAmount);
    public decimal TotalRake => Transactions.Sum(t => t.RakeAmount);
    public decimal TotalRakeCommission => Transactions.Sum(t => t.RakeCommission);
    public decimal? TotalRakeBack => Transactions.Sum(t => t.RakeBack ?? 0);
    public decimal TotalNetSettlement => Transactions.Sum(t => t.NetSettlementAmount);
}