using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Closing;

namespace SFManagement.Services;

public class ClosingNicknameService : BaseService<ClosingNickname>
{
    public ClosingNicknameService(DataContext context, IHttpContextAccessor httpContextAccessor) : base(context,
        httpContextAccessor)
    {
    }

    public async Task<List<IGrouping<Guid, ClosingNickname>>> GetByClosingManagerId(Guid closingManagerId)
    {
        return await _entity
            .Where(x => !x.DeletedAt.HasValue && x.ClosingManagerId == closingManagerId)
            .Include(x => x.Nickname)
            .GroupBy(x => x.Nickname.WalletId)
            .ToListAsync();
    }


    public override async Task<ClosingNickname> Update(Guid id, ClosingNickname obj)
    {
        var existing = await context.ClosingNicknames.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null) throw new AppException("Not found closing nickname.");

        existing.FatherNicknameId = obj.FatherNicknameId;
        existing.Rake = obj.Rake;
        existing.Balance = obj.Balance;
        existing.Rakeback = obj.Rakeback;
        existing.FatherPercentual = obj.FatherPercentual;

        context.Update(existing);

        await context.SaveChangesAsync();

        return existing;
    }
}