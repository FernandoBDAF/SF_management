using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.Support;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class FinancialBehaviorService : BaseService<FinancialBehavior>
{
    public FinancialBehaviorService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    // public async Task<BalanceResponse> GetBalance(Guid financialBehaviorId)
    // {
    //     var financialBehavior = await context.FinancialBehaviors.Include(x => x.BankTransactions).Include(x => x.WalletTransactions)
    //         // .Include(x => x.InternalTransactions)
    //         .FirstOrDefaultAsync(x => x.Id == financialBehaviorId);
    //
    //     return new BalanceResponse(financialBehavior);
    // }

    public override async Task<List<FinancialBehavior>> List()
    {
        var query = await context.FinancialBehaviors.Where(x => !x.FinancialBehaviorId.HasValue).ToListAsync();

        foreach (var financialBehavior in query) await GetChildren(financialBehavior);

        return query;
    }

    private async Task<List<FinancialBehavior>> GetChildren(FinancialBehavior financialBehavior)
    {
        var chds = await context.FinancialBehaviors
            .Where(x => x.FinancialBehaviorId == financialBehavior.Id && x.DeletedAt == null)
            .ToListAsync();

        foreach (var chd in chds) await GetChildren(chd);

        // financialBehavior.Children.AddRange(chds);

        return null;
    }
}