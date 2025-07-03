using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Transactions;

namespace SFManagement.Services;

public class SettlementTransactionService : BaseTransactionService<SettlementTransaction>
{
    public SettlementTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public async Task<Dictionary<DateTime, List<SettlementTransaction>>> GetClosings(Guid pokerManagerId)
    {
        var transactions = await context.SettlementTransactions
            .Include(st => st.AssetWallet)
            .Include(st => st.WalletIdentifier)
            .Where(st => st.DeletedAt == null && 
                        st.AssetWallet.BaseAssetHolderId == pokerManagerId)
            .OrderBy(st => st.Date)
            .ToListAsync();

        var groupedTransactions = transactions
            .GroupBy(st => st.Date.Date) // Group by date only (without time)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );

        return groupedTransactions;
    }
}