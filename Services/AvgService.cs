using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class AvgRateService : BaseService<AvgRate>
{
    private readonly WalletTransactionService _walletTransactionService;

    public AvgRateService(DataContext context, IHttpContextAccessor httpContextAccessor,
        WalletTransactionService walletTransactionService) : base(context, httpContextAccessor)
    {
        _walletTransactionService = walletTransactionService;
    }

    public async Task Reset(Guid managerId)
    {
        await Task.Yield();
        // var firstDate = await context.DigitalAssetTransactions
        //     .Where(x => x.Wallet.ManagerId == managerId)
        //     .OrderBy(x => x.Date)
        //     .FirstOrDefaultAsync();
        // await _walletTransactionService.CalcAvgRate(
        //     await context.Managers.FirstOrDefaultAsync(x => x.Id == managerId), firstDate.Date);
    }

    public async Task Calc(Guid managerId, DateTime referenceDate)
    {
        await Task.Yield();
        // await _walletTransactionService.CalcAvgRate(
        //     await context.Managers.FirstOrDefaultAsync(x => x.Id == managerId), referenceDate.Date);
    }
}