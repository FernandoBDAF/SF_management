using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

public class SettlementTransactionRequest : BaseTransactionRequest
{
    public decimal RakeAmount { get; set; }
    
    public decimal RakeCommission { get; set; }
    
    public decimal? RakeBack { get; set; }
}

public class ReducedSettlementTransactionRequest
{
    public decimal AssetAmount { get; set; }

    public decimal RakeAmount { get; set; }
    
    public decimal RakeCommission { get; set; }
    
    public decimal? RakeBack { get; set; }

    public Guid SenderWalletIdentifierId { get; set; }
    public Guid ReceiverWalletIdentifierId { get; set; }
}

public class SettlementTransactionByDateRequest
{
    public DateTime Date { get; set; }

    public AssetType AssetType { get; set; }

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