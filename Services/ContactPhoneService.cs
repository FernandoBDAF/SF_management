using SFManagement.Data;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class ContactPhoneService: BaseService<ContactPhone>
{
    public ContactPhoneService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public override async Task<ContactPhone> Add(ContactPhone obj)
    {
        EnforceSingleOwner(obj);
        return await base.Add(obj);
    }

    public override async Task<ContactPhone> Update(Guid id, ContactPhone obj)
    {
        EnforceSingleOwner(obj);
        return await base.Update(id, obj);
    }

    private static void EnforceSingleOwner(ContactPhone contactPhone)
    {
        var ownerCount = new[] { contactPhone.ClientId, contactPhone.MemberId, contactPhone.PokerManagerId }
            .Count(id => id != null);
        if (ownerCount != 1)
            throw new InvalidOperationException("ContactPhone must be linked to exactly one owner (Client, Bank, Member, or PokerManager).");
    }
}