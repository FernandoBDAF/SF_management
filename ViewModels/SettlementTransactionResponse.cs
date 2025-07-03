using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class SettlementTransactionResponse : BaseTransactionResponse
{
    public decimal AssetAmount { get; set; }
    
    public TransactionDirection? TransactionDirection { get; set; }

    public decimal Rake { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
}