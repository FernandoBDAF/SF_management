using SFManagement.Data;
using SFManagement.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Services;

public class AgencyInvoiceService : BaseService<AgencyInvoice>
{
    private readonly FiatAssetTransactionService _fiatAssetTransactionService;
    public AgencyInvoiceService(DataContext context, IHttpContextAccessor httpContextAccessor, FiatAssetTransactionService fiatAssetTransactionService) : base(context, httpContextAccessor)
    {
        _fiatAssetTransactionService = fiatAssetTransactionService;
    }

    public async void GenerateCluster(Guid baseAssetHolderId)
    {
        // improvement: check the type of invoce generation of the baseAssetHolder
        var agencyInvoice = await context.AgencyInvoices
        .Where(x => x.BaseAssetHolderId == baseAssetHolderId)
        .Where(x => x.ClosedAt != null)
        .OrderBy(x => x.CreatedAt)
        .FirstOrDefaultAsync();

        var expenseTransactions = await context.FiatAssetTransactions
        .Include(x => x.AssetWallet)
        .ThenInclude(x => x.BaseAssetHolder)
        .Where(x => x.AssetWallet.BaseAssetHolderId == baseAssetHolderId && 
        x.AgencyInvoiceId == null &&
        x.AssetWallet.AssetType != 0 &&
        x.TransactionDirection == TransactionDirection.Expense)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();

        var incomeTransactions = await context.FiatAssetTransactions
        .Include(x => x.AssetWallet)
        .ThenInclude(x => x.BaseAssetHolder)
        .Where(x => x.AssetWallet.BaseAssetHolderId == baseAssetHolderId &&
        x.AgencyInvoiceId == null &&
        x.AssetWallet.AssetType != 0 &&
        x.TransactionDirection == TransactionDirection.Income)
        .OrderBy(x => x.CreatedAt)
        .ToListAsync();

        var i = 0;
        var j = 0;

        while (i < expenseTransactions.Count && j < incomeTransactions.Count)
        {
            // optimization: create list by tax entity type and consume the not taxable first
            var incomeTransaction = incomeTransactions[i];
            var expenseTransaction = expenseTransactions[j];

            agencyInvoice ??= await base.Add(new AgencyInvoice
                {
                    BaseAssetHolderId = baseAssetHolderId
                });

            if (incomeTransaction.AssetWallet.BaseAssetHolder.TaxEntityType == TaxEntityType.CPF ||
            incomeTransaction.AssetWallet.BaseAssetHolder.TaxEntityType == TaxEntityType.CNPJ)
            {
                incomeTransaction.AgencyInvoiceId = agencyInvoice.Id;
                var FiatBalance = incomeTransaction.AssetAmount;
                while (FiatBalance > 0 && j < expenseTransactions.Count)
                {
                    expenseTransaction = expenseTransactions[j];
                    expenseTransaction.AgencyInvoiceId = agencyInvoice.Id;
                    FiatBalance -= expenseTransaction.AssetAmount;
                    if (FiatBalance <= 0)
                    {
                        agencyInvoice.ReminderId = expenseTransaction.Id;
                        agencyInvoice.ClosedAt = DateTime.UtcNow;
                        await base.Update(agencyInvoice.Id, agencyInvoice);
                        // update income and all expense transactions
                        await _fiatAssetTransactionService.Update(incomeTransaction.Id, incomeTransaction);
                        await _fiatAssetTransactionService.Update(expenseTransaction.Id, expenseTransaction);
                        await context.SaveChangesAsync();
                        continue;
                    }
                    j++;
                }

            }
            else if (expenseTransaction.AssetWallet.BaseAssetHolder.TaxEntityType == TaxEntityType.CPF ||
            expenseTransaction.AssetWallet.BaseAssetHolder.TaxEntityType == TaxEntityType.CNPJ)
            {

            }
            else
            {
                if (i <= j)
                {
                    i++;
                    continue;
                }
                j++;
                continue;
            }

            
        }
    }
}