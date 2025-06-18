using SFManagement.Data;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class InitialBalanceService : BaseService<InitialBalance>
{
    public InitialBalanceService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public override async Task<InitialBalance> Add(InitialBalance obj)
    {
        EnforceSingleOwner(obj);
        return await base.Add(obj);
    }

    public override async Task<InitialBalance> Update(Guid id, InitialBalance obj)
    {
        EnforceSingleOwner(obj);
        return await base.Update(id, obj);
    }

    private static void EnforceSingleOwner(InitialBalance initialBalance)
    {
        var ownerCount = new[] { initialBalance.ClientId, initialBalance.MemberId, initialBalance.PokerManagerId }
            .Count(id => id != null);
        if (ownerCount != 1)
            throw new InvalidOperationException("InitialBalance must be linked to exactly one owner (Client, Bank, Member, or PokerManager).");
    }
}