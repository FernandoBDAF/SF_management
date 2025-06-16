using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.Services;

public class BalanceService
{
    private readonly DataContext _context;
    public readonly DbSet<DigitalAssetTransaction> _digitalAssetTransactionsEntity;
    public readonly DbSet<FiatAssetTransaction> _fiatAssetTransactionsEntity;

    public BalanceService(DataContext context)
    {
        this._context = context;
        _digitalAssetTransactionsEntity = context.Set<DigitalAssetTransaction>();
        _fiatAssetTransactionsEntity = context.Set<FiatAssetTransaction>();
    }
}