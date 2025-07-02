using SFManagement.Data;
using SFManagement.Models.Support;

namespace SFManagement.Services;

public class ContactPhoneService: BaseService<ContactPhone>
{
    public ContactPhoneService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
}