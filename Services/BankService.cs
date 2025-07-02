using SFManagement.Data;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class BankService(DataContext context, IHttpContextAccessor httpContextAccessor) : BaseAssetHolderService<Bank>(context,
    httpContextAccessor)
{
    
    // public override async Task<Bank> Update(Guid id, Bank obj)
    // {
    //     var existing = await context.Banks.FirstOrDefaultAsync(x => x.Id == id);

    //     if (existing == null) throw new AppException("Not found bank");

    //     // existing.InitialValue = obj.InitialValue;
    //     existing.Code = obj.Code;
    //     existing.Name = obj.Name;

    //     context.Banks.Update(existing);

    //     await context.SaveChangesAsync();

    //     return obj;
    // }
}