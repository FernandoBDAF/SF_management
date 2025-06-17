using SFManagement.Data;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class AddressService: BaseService<Address>
{
    public AddressService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public override async Task<Address> Add(Address obj)
    {
        EnforceSingleOwner(obj);
        return await base.Add(obj);
    }

    public override async Task<Address> Update(Guid id, Address obj)
    {
        EnforceSingleOwner(obj);
        return await base.Update(id, obj);
    }

    private static void EnforceSingleOwner(Address address)
    {
        var ownerCount = new[] { address.ClientId,  address.MemberId, address.PokerManagerId }
            .Count(id => id != null);
        if (ownerCount != 1)
            throw new InvalidOperationException("Address must be linked to exactly one owner (Client, Bank, Member, or PokerManager).");
    }
}