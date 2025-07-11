using SFManagement.Enums;

namespace SFManagement.ViewModels;

public class SettlementTransactionRequest : BaseTransactionRequest
{
    public decimal Rake { get; set; }
    
    public decimal RakeCommission { get; set; }
    
    public decimal? RakeBack { get; set; }
}

public class ReducedSettlementTransactionRequest
{
    public decimal AssetAmount { get; set; }

    public decimal Rake { get; set; }
    
    public decimal RakeCommission { get; set; }
    
    public decimal? RakeBack { get; set; }

    // Using new transaction model - receiver wallet identifier (client/player)
    public Guid? ReceiverWalletIdentifierId { get; set; }
}

public class SettlementTransactionByDateRequest
{
    public DateTime Date { get; set; }

    public Guid? AssetPoolId { get; set; }

    public List<ReducedSettlementTransactionRequest> Transactions { get; set; } = [];
}

public class SettlementTransactionByDateResponse
{
    public bool Success { get; set; }
    public List<SettlementTransactionResponse> CreatedTransactions { get; set; } = new();
    public List<SettlementTransactionError> Errors { get; set; } = new();
    public string? Message { get; set; }
}

public class SettlementTransactionError
{
    public int Index { get; set; }
    public string Error { get; set; } = string.Empty;
    public SettlementTransactionRequest? Transaction { get; set; }
}