using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class AvgRateService : BaseService<AvgRate>
{
    private readonly DigitalAssetTransactionService _digitalAssetTransactionService;

    public AvgRateService(DataContext context, IHttpContextAccessor httpContextAccessor,
        DigitalAssetTransactionService digitalAssetTransactionService) : base(context, httpContextAccessor)
    {
        _digitalAssetTransactionService = digitalAssetTransactionService;
    }

    public async Task Reset(Guid managerId)
    {
        await Task.Yield();
        // var firstDate = await context.DigitalAssetTransactions
        //     .Where(x => x.AssetWallet.ManagerId == managerId)
        //     .OrderBy(x => x.Date)
        //     .FirstOrDefaultAsync();
        // await _digitalAssetTransactionService.CalcAvgRate(
        //     await context.Managers.FirstOrDefaultAsync(x => x.Id == managerId), firstDate.Date);
    }

    public async Task Calc(Guid managerId, DateTime referenceDate)
    {
        await Task.Yield();
        // await _digitalAssetTransactionService.CalcAvgRate(
        //     await context.Managers.FirstOrDefaultAsync(x => x.Id == managerId), referenceDate.Date);
    }
}