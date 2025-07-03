namespace SFManagement.ViewModels;

public class SettlementTransactionRequest : BaseTransactionRequest
{
    public decimal Rake { get; set; }
    
    public decimal RakeCommission { get; set; }
    
    public decimal? RakeBack { get; set; }
}