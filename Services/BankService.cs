// using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class BankService(DataContext context, IHttpContextAccessor httpContextAccessor) : BaseAssetHolderService<Bank>(context,
    httpContextAccessor)
{
    public async Task<Guid[]> GetBankAssetWalletIds()
    {
        var bankAssetWalletIds = await context.Banks.Include(x => x.AssetWallets).Select(x => x.AssetWallets.Select(y => y.Id)).SelectMany(x => x).ToArrayAsync();
        return bankAssetWalletIds;
    }

    // public async Task<BalanceResponse> GetBalance(Guid bankId, DateTime? date)
    // {
    //     await Task.Yield();
    //     // var now = DateTime.Now;
    //     // if (!date.HasValue || date.Value.Year == 1) date = now;
    //     // var bank = await context.Banks.Include(x => x.BankTransactions).Include(x => x.InternalTransactions)
    //     //     .FirstOrDefaultAsync(x => x.Id == bankId);
    //     //
    //     // return new BalanceResponse(bank.BankTransactions, bank.InternalTransactions, bank.InitialValue, date);
    //     return null;
    // }


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