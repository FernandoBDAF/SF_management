using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;

namespace SFManagement.Services;

public class WalletIdentifierService : BaseService<WalletIdentifier>
{
    public WalletIdentifierService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }
}