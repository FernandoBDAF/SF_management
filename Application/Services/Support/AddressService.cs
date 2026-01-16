using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Infrastructure.Data;
using SFManagement.Infrastructure.Data;
using SFManagement.Domain.Entities.Support;

namespace SFManagement.Application.Services.Support;

public class AddressService: BaseService<Address>
{
    public AddressService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
}