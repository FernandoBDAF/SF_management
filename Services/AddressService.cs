using SFManagement.Data;
using SFManagement.Models.Support;

namespace SFManagement.Services;

public class AddressService: BaseService<Address>
{
    public AddressService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
}