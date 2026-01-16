using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Transactions;
namespace SFManagement.Application.DTOs.Statements;

public class StatementAssetHolderWithTransactions
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public virtual StatementTransactionResponse[]? Transactions { get; set; }
}