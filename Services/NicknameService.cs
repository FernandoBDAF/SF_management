using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;

namespace SFManagement.Services;

public class NicknameService : BaseService<Nickname>
{
    public NicknameService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public async Task<List<Nickname>> GetByClientId(Guid clientId)
    {
        return await context.Nicknames.Where(x => x.ClientId == clientId).ToListAsync();
    }
}