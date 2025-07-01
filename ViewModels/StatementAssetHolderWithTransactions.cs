namespace SFManagement.ViewModels;

public class StatementAssetHolderWithTransactions
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }
    
    public virtual StatementTransactionResponse[]? Transactions { get; set; }
}