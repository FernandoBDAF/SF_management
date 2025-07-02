using SFManagement.Data;
using SFManagement.Models.Support;

namespace SFManagement.Services;

public class InitialBalanceService : BaseService<InitialBalance>
{
    public InitialBalanceService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
}