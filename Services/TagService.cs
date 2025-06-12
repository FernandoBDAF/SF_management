using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class TagService : BaseService<Tag>
{
    public TagService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public async Task<BalanceResponse> GetBalance(Guid tagId)
    {
        var tag = await context.Tags.Include(x => x.BankTransactions).Include(x => x.WalletTransactions)
            .Include(x => x.InternalTransactions).FirstOrDefaultAsync(x => x.Id == tagId);

        return new BalanceResponse(tag);
    }

    public override async Task<List<Tag>> List()
    {
        var query = await context.Tags.Where(x => !x.ParentId.HasValue).ToListAsync();

        foreach (var tag in query) await GetChildren(tag);

        return query;
    }

    private async Task<List<Tag>> GetChildren(Tag tag)
    {
        var chds = await context.Tags.Where(x => x.ParentId == tag.Id).ToListAsync();

        foreach (var chd in chds) await GetChildren(chd);

        // tag.Children.AddRange(chds);

        return null;
    }
}