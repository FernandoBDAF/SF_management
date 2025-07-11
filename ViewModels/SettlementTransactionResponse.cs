using SFManagement.Enums;
using SFManagement.Models;

namespace SFManagement.ViewModels;

public class SettlementTransactionResponse : BaseTransactionResponse
{
    public decimal AssetAmount { get; set; }
    
    // public TransactionDirection? TransactionDirection { get; set; }

    public decimal Rake { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
}

public class SettlementTransactionSimplifiedResponse : BaseResponse
{
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public virtual CategoryResponse? FinancialBehavior { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    
    public decimal AssetAmount { get; set; }
    // public TransactionDirection? TransactionDirection { get; set; }
    public decimal Rake { get; set; }
    public decimal RakeCommission { get; set; }
    public decimal? RakeBack { get; set; }
}