using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.Support;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Support;

public class ContactPhoneService: BaseService<ContactPhone>
{
    public ContactPhoneService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
}