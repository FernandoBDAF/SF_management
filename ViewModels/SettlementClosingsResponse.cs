using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class SettlementClosingsGroupedResponse
{
    public List<SettlementClosingGroup> ClosingGroups { get; set; } = new();
}

public class SettlementClosingGroup
{
    public DateTime Date { get; set; }
    public List<SettlementTransactionResponse> Transactions { get; set; } = new();
    public int TransactionCount => Transactions.Count;

    // public decimal TotalAssetAmount => Transactions.Sum(t => t.TransactionDirection == TransactionDirection.Income ? t.AssetAmount : -t.AssetAmount);
    public decimal TotalRake => Transactions.Sum(t => t.Rake);
    public decimal? TotalCommission => Transactions.Sum(t => t.Rake * (t.RakeCommission - t.RakeBack - (t.WalletIdentifier?.DefaultParentCommission ?? 0)));
} 