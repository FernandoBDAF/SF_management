using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class WalletIdentifierService : BaseService<WalletIdentifier>
{
    public WalletIdentifierService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
    
    public override async Task<WalletIdentifier> Add(WalletIdentifier obj)
    {
        EnforceSingleOwner(obj);
        return await base.Add(obj);
    }

    public override async Task<WalletIdentifier> Update(Guid id, WalletIdentifier obj)
    {
        EnforceSingleOwner(obj);
        return await base.Update(id, obj);
    }

    private static void EnforceSingleOwner(WalletIdentifier address)
    {
        var ownerCount = new[] { address.ClientId, address.MemberId, address.PokerManagerId, address.BankId }
            .Count(id => id != null);
        if (ownerCount != 1)
            throw new InvalidOperationException("WalletIdentifier must be linked to exactly one owner (Client, Bank, Member, or PokerManager).");
    }

    public async Task<List<WalletIdentifier>> GetByClientId(Guid clientId)
    {
        await Task.Yield();
        return null;
        // return await context.WalletIdentifiers.Where(x => x.AssetHolderId == clientId).ToListAsync();
    }
}