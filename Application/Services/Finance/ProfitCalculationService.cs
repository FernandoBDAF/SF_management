using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SFManagement.Application.DTOs.Finance;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Finance;

public class ProfitCalculationService : IProfitCalculationService
{
    private readonly DataContext _context;
    private readonly IAvgRateService _avgRateService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ProfitCalculationService> _logger;
    private static readonly TimeSpan SystemWalletCacheDuration = TimeSpan.FromMinutes(10);
    private const string SystemWalletCacheKey = "finance.system-wallet-ids";
    private static readonly TimeSpan ManagerProfitTypeCacheDuration = TimeSpan.FromMinutes(10);
    private const string RakeManagersCacheKey = "profit.rake-manager-ids";
    private const string SpreadManagersCacheKey = "profit.spread-manager-ids";

    public ProfitCalculationService(
        DataContext context,
        IAvgRateService avgRateService,
        IMemoryCache cache,
        ILogger<ProfitCalculationService> logger)
    {
        _context = context;
        _avgRateService = avgRateService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ProfitSummary> GetProfitSummary(
        DateTime startDate,
        DateTime endDate,
        Guid? managerId = null)
    {
        var normalizedManagerId = await NormalizeManagerBaseAssetHolderId(managerId);

        _logger.LogInformation(
            "Calculating profit summary from {Start:yyyy-MM-dd} to {End:yyyy-MM-dd}, ManagerId={ManagerId}, NormalizedManagerId={NormalizedManagerId}",
            startDate, endDate, managerId, normalizedManagerId);

        var directIncome = await CalculateDirectIncome(startDate, endDate, normalizedManagerId);
        var rakeCommission = await CalculateRakeCommission(startDate, endDate, normalizedManagerId);
        var rateFees = await CalculateRateFees(startDate, endDate, normalizedManagerId);
        var spreadProfit = await CalculateSpreadProfit(startDate, endDate, normalizedManagerId);

        return new ProfitSummary
        {
            StartDate = startDate,
            EndDate = endDate,
            ManagerId = normalizedManagerId,
            DirectIncome = directIncome,
            RakeCommission = rakeCommission,
            RateFees = rateFees,
            SpreadProfit = spreadProfit
        };
    }

    public async Task<List<ProfitByManager>> GetProfitByManager(DateTime startDate, DateTime endDate)
    {
        var managers = await _context.PokerManagers
            .AsNoTracking()
            .Include(m => m.BaseAssetHolder)
            .Where(m => !m.DeletedAt.HasValue)
            .ToListAsync();

        var results = new List<ProfitByManager>();

        foreach (var manager in managers)
        {
            var summary = await GetProfitSummary(startDate, endDate, manager.BaseAssetHolderId);
            results.Add(new ProfitByManager
            {
                ManagerId = manager.BaseAssetHolderId,
                ManagerName = manager.BaseAssetHolder?.Name ?? "Unknown",
                ManagerProfitType = manager.ManagerProfitType,
                DirectIncome = summary.DirectIncome,
                RakeCommission = summary.RakeCommission,
                RateFees = summary.RateFees,
                SpreadProfit = summary.SpreadProfit,
                TotalProfit = summary.TotalProfit
            });
        }

        return results.OrderByDescending(p => p.TotalProfit).ToList();
    }

    public async Task<List<ProfitBySource>> GetProfitBySource(DateTime startDate, DateTime endDate)
    {
        var summary = await GetProfitSummary(startDate, endDate);

        return new List<ProfitBySource>
        {
            new() { Source = "DirectIncome", Amount = summary.DirectIncome },
            new() { Source = "RakeCommission", Amount = summary.RakeCommission },
            new() { Source = "RateFees", Amount = summary.RateFees },
            new() { Source = "SpreadProfit", Amount = summary.SpreadProfit }
        };
    }

    public async Task<DirectIncomeDetailsResponse> GetDirectIncomeDetails(
        DateTime startDate,
        DateTime endDate)
    {
        _logger.LogInformation(
            "Getting direct income details from {Start:yyyy-MM-dd} to {End:yyyy-MM-dd}",
            startDate, endDate);

        var systemWalletIds = await GetSystemWalletIds();

        var incomes = new List<DirectIncomeItem>();
        var expenses = new List<DirectIncomeItem>();

        if (!systemWalletIds.Any())
        {
            _logger.LogDebug("No system wallets found for Direct Income details");
            return new DirectIncomeDetailsResponse
            {
                StartDate = startDate,
                EndDate = endDate,
                Incomes = incomes,
                Expenses = expenses,
                TotalIncome = 0,
                TotalExpense = 0,
                NetDirectIncome = 0
            };
        }

        await ProcessFiatTransactions(startDate, endDate, systemWalletIds, incomes, expenses);
        await ProcessDigitalTransactions(startDate, endDate, systemWalletIds, incomes, expenses);

        incomes = incomes.OrderByDescending(item => item.Date).ToList();
        expenses = expenses.OrderByDescending(item => item.Date).ToList();

        var totalIncome = incomes.Sum(item => item.Amount);
        var totalExpense = expenses.Sum(item => item.Amount);

        return new DirectIncomeDetailsResponse
        {
            StartDate = startDate,
            EndDate = endDate,
            Incomes = incomes,
            Expenses = expenses,
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetDirectIncome = totalIncome - totalExpense
        };
    }

    private async Task<decimal> CalculateDirectIncome(
        DateTime startDate,
        DateTime endDate,
        Guid? managerId)
    {
        var systemWalletIds = await GetSystemWalletIds();
        
        if (!systemWalletIds.Any())
        {
            _logger.LogDebug("No system wallets found for Direct Income calculation");
            return 0;
        }
        
        decimal totalDirectIncome = 0;
        
        var fiatTransactions = await _context.FiatAssetTransactions
            .AsNoTracking()
            .Include(t => t.SenderWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Include(t => t.ReceiverWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= startDate
                && t.Date <= endDate
                && t.CategoryId.HasValue
                && (systemWalletIds.Contains(t.SenderWalletIdentifierId)
                    || systemWalletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();
        
        foreach (var tx in fiatTransactions)
        {
            var systemIsReceiver = systemWalletIds.Contains(tx.ReceiverWalletIdentifierId);
            
            if (systemIsReceiver)
                totalDirectIncome += tx.AssetAmount;
            else
                totalDirectIncome -= tx.AssetAmount;
        }
        
        var digitalTransactions = await _context.DigitalAssetTransactions
            .AsNoTracking()
            .Include(t => t.SenderWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Include(t => t.ReceiverWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= startDate
                && t.Date <= endDate
                && t.CategoryId.HasValue
                && (systemWalletIds.Contains(t.SenderWalletIdentifierId)
                    || systemWalletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();
        
        foreach (var tx in digitalTransactions)
        {
            var systemIsReceiver = systemWalletIds.Contains(tx.ReceiverWalletIdentifierId);
            
            var nonSystemAssetHolderId = systemIsReceiver
                ? tx.SenderWalletIdentifier?.AssetPool?.BaseAssetHolderId
                : tx.ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolderId;
            
            var brlValue = tx.AssetAmount;
            if (nonSystemAssetHolderId.HasValue)
            {
                var avgRate = await _avgRateService.GetAvgRateAtDate(nonSystemAssetHolderId.Value, tx.Date);
                brlValue = tx.AssetAmount * avgRate;
            }
            
            if (systemIsReceiver)
                totalDirectIncome += brlValue;
            else
                totalDirectIncome -= brlValue;
        }
        
        _logger.LogDebug("Direct Income: {Amount:C}", totalDirectIncome);
        return totalDirectIncome;
    }

    private async Task ProcessFiatTransactions(
        DateTime startDate,
        DateTime endDate,
        List<Guid> systemWalletIds,
        List<DirectIncomeItem> incomes,
        List<DirectIncomeItem> expenses)
    {
        var fiatTransactions = await _context.FiatAssetTransactions
            .AsNoTracking()
            .Include(t => t.SenderWalletIdentifier)
                .ThenInclude(w => w!.AssetPool)
                    .ThenInclude(p => p!.BaseAssetHolder)
            .Include(t => t.ReceiverWalletIdentifier)
                .ThenInclude(w => w!.AssetPool)
                    .ThenInclude(p => p!.BaseAssetHolder)
            .Include(t => t.Category)
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= startDate
                && t.Date <= endDate
                && t.CategoryId.HasValue
                && (systemWalletIds.Contains(t.SenderWalletIdentifierId)
                    || systemWalletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();

        foreach (var tx in fiatTransactions)
        {
            var systemIsReceiver = systemWalletIds.Contains(tx.ReceiverWalletIdentifierId);
            var nonSystemWallet = systemIsReceiver
                ? tx.SenderWalletIdentifier
                : tx.ReceiverWalletIdentifier;
            var origin = nonSystemWallet?.AssetPool?.BaseAssetHolder?.Name ?? "Desconhecido";

            var item = new DirectIncomeItem
            {
                Id = tx.Id,
                Date = tx.Date,
                Description = tx.Category?.Description ?? "Transação",
                Amount = Math.Abs(tx.AssetAmount),
                Origin = origin,
                TransactionType = DirectIncomeTransactionType.FiatAsset,
                CategoryId = tx.CategoryId
            };

            if (systemIsReceiver)
                incomes.Add(item);
            else
                expenses.Add(item);
        }
    }

    private async Task ProcessDigitalTransactions(
        DateTime startDate,
        DateTime endDate,
        List<Guid> systemWalletIds,
        List<DirectIncomeItem> incomes,
        List<DirectIncomeItem> expenses)
    {
        var digitalTransactions = await _context.DigitalAssetTransactions
            .AsNoTracking()
            .Include(t => t.SenderWalletIdentifier)
                .ThenInclude(w => w!.AssetPool)
                    .ThenInclude(p => p!.BaseAssetHolder)
            .Include(t => t.ReceiverWalletIdentifier)
                .ThenInclude(w => w!.AssetPool)
                    .ThenInclude(p => p!.BaseAssetHolder)
            .Include(t => t.Category)
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= startDate
                && t.Date <= endDate
                && t.CategoryId.HasValue
                && (systemWalletIds.Contains(t.SenderWalletIdentifierId)
                    || systemWalletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();

        foreach (var tx in digitalTransactions)
        {
            var systemIsReceiver = systemWalletIds.Contains(tx.ReceiverWalletIdentifierId);
            var nonSystemWallet = systemIsReceiver
                ? tx.SenderWalletIdentifier
                : tx.ReceiverWalletIdentifier;
            var origin = nonSystemWallet?.AssetPool?.BaseAssetHolder?.Name ?? "Desconhecido";
            var nonSystemAssetHolderId = nonSystemWallet?.AssetPool?.BaseAssetHolderId;

            var brlAmount = tx.AssetAmount;
            if (nonSystemAssetHolderId.HasValue)
            {
                var avgRate = await _avgRateService.GetAvgRateAtDate(nonSystemAssetHolderId.Value, tx.Date);
                brlAmount = tx.AssetAmount * avgRate;
            }

            var item = new DirectIncomeItem
            {
                Id = tx.Id,
                Date = tx.Date,
                Description = tx.Category?.Description ?? "Transação Digital",
                Amount = Math.Abs(brlAmount),
                Origin = origin,
                TransactionType = DirectIncomeTransactionType.DigitalAsset,
                CategoryId = tx.CategoryId
            };

            if (systemIsReceiver)
                incomes.Add(item);
            else
                expenses.Add(item);
        }
    }

    private async Task<decimal> CalculateRakeCommission(
        DateTime startDate,
        DateTime endDate,
        Guid? managerId)
    {
        var rakeManagerIds = await GetManagerIdsByProfitType(ManagerProfitType.RakeOverrideCommission);
        
        if (managerId.HasValue && !rakeManagerIds.Contains(managerId.Value))
            return 0;
        
        var targetManagerIds = managerId.HasValue
            ? new List<Guid> { managerId.Value }
            : rakeManagerIds;
        
        if (!targetManagerIds.Any())
            return 0;
        
        var managerWalletIds = await _context.WalletIdentifiers
            .AsNoTracking()
            .Include(w => w.AssetPool)
            .Where(w => targetManagerIds.Contains(w.AssetPool!.BaseAssetHolderId!.Value)
                && !w.DeletedAt.HasValue)
            .Select(w => w.Id)
            .ToListAsync();
        
        var settlements = await _context.SettlementTransactions
            .AsNoTracking()
            .Include(t => t.SenderWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Include(t => t.ReceiverWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= startDate
                && t.Date <= endDate
                && t.RakeAmount > 0
                && (managerWalletIds.Contains(t.SenderWalletIdentifierId)
                    || managerWalletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();
        
        decimal totalRakeProfit = 0;
        
        foreach (var s in settlements)
        {
            var rakeChips = s.RakeAmount * ((s.RakeCommission - (s.RakeBack ?? 0)) / 100m);
            
            var mgrId = s.SenderWalletIdentifier?.AssetPool?.BaseAssetHolderId
                ?? s.ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolderId;
            
            if (mgrId.HasValue && targetManagerIds.Contains(mgrId.Value))
            {
                var avgRate = await _avgRateService.GetAvgRateAtDate(mgrId.Value, s.Date);
                totalRakeProfit += rakeChips * avgRate;
            }
        }
        
        _logger.LogDebug("Rake Commission: {Amount:C}", totalRakeProfit);
        return totalRakeProfit;
    }

    private async Task<decimal> CalculateRateFees(
        DateTime startDate,
        DateTime endDate,
        Guid? managerId)
    {
        var activeManagerIds = await _context.PokerManagers
            .AsNoTracking()
            .Where(m => !m.DeletedAt.HasValue)
            .Select(m => m.BaseAssetHolderId)
            .ToListAsync();
        var activeManagerIdSet = activeManagerIds.ToHashSet();

        var query = _context.DigitalAssetTransactions
            .AsNoTracking()
            .Include(t => t.SenderWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Include(t => t.ReceiverWalletIdentifier).ThenInclude(w => w!.AssetPool)
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= startDate
                && t.Date <= endDate
                && t.Rate.HasValue
                && t.Rate.Value != 0);

        if (managerId.HasValue)
        {
            query = query.Where(t =>
                t.SenderWalletIdentifier!.AssetPool!.BaseAssetHolderId == managerId.Value ||
                t.ReceiverWalletIdentifier!.AssetPool!.BaseAssetHolderId == managerId.Value);
        }

        var transactions = await query.ToListAsync();
        decimal totalFeeProfit = 0;

        foreach (var tx in transactions)
        {
            var feeInChips = tx.AssetAmount * (tx.Rate!.Value / (100m + tx.Rate.Value));

            var senderHolderId = tx.SenderWalletIdentifier?.AssetPool?.BaseAssetHolderId;
            var receiverHolderId = tx.ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolderId;

            Guid? txManagerId = null;
            if (senderHolderId.HasValue && activeManagerIdSet.Contains(senderHolderId.Value))
            {
                txManagerId = senderHolderId.Value;
            }
            else if (receiverHolderId.HasValue && activeManagerIdSet.Contains(receiverHolderId.Value))
            {
                txManagerId = receiverHolderId.Value;
            }

            if (txManagerId.HasValue)
            {
                var avgRate = await _avgRateService.GetAvgRateAtDate(txManagerId.Value, tx.Date);
                totalFeeProfit += feeInChips * avgRate;
            }
        }

        _logger.LogDebug("Rate Fees: {Amount:C}", totalFeeProfit);
        return totalFeeProfit;
    }

    private async Task<decimal> CalculateSpreadProfit(
        DateTime startDate,
        DateTime endDate,
        Guid? managerId)
    {
        var spreadManagerIds = await GetManagerIdsByProfitType(ManagerProfitType.Spread);
        
        if (managerId.HasValue && !spreadManagerIds.Contains(managerId.Value))
            return 0;
        
        var targetManagerIds = managerId.HasValue
            ? new List<Guid> { managerId.Value }
            : spreadManagerIds;
        
        if (!targetManagerIds.Any())
            return 0;
        
        decimal totalSpreadProfit = 0;
        
        foreach (var mgrId in targetManagerIds)
        {
            var walletIds = await _context.WalletIdentifiers
                .AsNoTracking()
                .Include(w => w.AssetPool)
                .Where(w => w.AssetPool!.BaseAssetHolderId == mgrId
                    && w.AssetPool.AssetGroup == AssetGroup.PokerAssets
                    && !w.DeletedAt.HasValue)
                .Select(w => w.Id)
                .ToListAsync();
            
            if (!walletIds.Any()) continue;
            
            var sales = await _context.DigitalAssetTransactions
                .AsNoTracking()
                .Where(t => !t.DeletedAt.HasValue
                    && t.Date >= startDate
                    && t.Date <= endDate
                    && walletIds.Contains(t.SenderWalletIdentifierId)
                    && t.ConversionRate.HasValue)
                .ToListAsync();
            
            foreach (var sale in sales)
            {
                var avgRate = await _avgRateService.GetAvgRateAtDate(mgrId, sale.Date);
                if (avgRate == 0)
                {
                    _logger.LogWarning(
                        "AvgRate is 0 for manager {ManagerId} at {Date}. Spread profit may be overstated.",
                        mgrId, sale.Date);
                }

                var profit = sale.AssetAmount * (sale.ConversionRate!.Value - avgRate);
                totalSpreadProfit += profit;
            }
        }
        
        _logger.LogDebug("Spread Profit: {Amount:C}", totalSpreadProfit);
        return totalSpreadProfit;
    }
    
    private async Task<List<Guid>> GetSystemWalletIds()
    {
        if (_cache.TryGetValue(SystemWalletCacheKey, out List<Guid>? cachedWalletIds))
        {
            return cachedWalletIds ?? new List<Guid>();
        }
        
        // Optimization note:
        // We avoid navigation-property filtering + Include here because it causes a slow query
        // on large datasets. A direct join against AssetPools is simpler and runs faster.
        var walletIds = await (
                from wallet in _context.WalletIdentifiers.AsNoTracking()
                join pool in _context.AssetPools.AsNoTracking()
                    on wallet.AssetPoolId equals pool.Id
                where !wallet.DeletedAt.HasValue
                      && !pool.DeletedAt.HasValue
                      && pool.AssetGroup == AssetGroup.Internal
                      && pool.BaseAssetHolderId == null
                select wallet.Id
            )
            .ToListAsync();
        
        _cache.Set(SystemWalletCacheKey, walletIds, SystemWalletCacheDuration);
        
        return walletIds;
    }

    private async Task<List<Guid>> GetManagerIdsByProfitType(ManagerProfitType profitType)
    {
        var cacheKey = profitType == ManagerProfitType.RakeOverrideCommission
            ? RakeManagersCacheKey
            : SpreadManagersCacheKey;

        if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedIds))
        {
            return cachedIds ?? new List<Guid>();
        }

        var managerIds = await _context.PokerManagers
            .AsNoTracking()
            .Where(m => m.ManagerProfitType == profitType && !m.DeletedAt.HasValue)
            .Select(m => m.BaseAssetHolderId)
            .ToListAsync();

        _cache.Set(cacheKey, managerIds, ManagerProfitTypeCacheDuration);
        return managerIds;
    }

    /// <summary>
    /// Profit endpoints are centered on BaseAssetHolderId. To avoid client-side confusion,
    /// this method accepts either BaseAssetHolderId or PokerManager.Id and normalizes to BaseAssetHolderId.
    /// </summary>
    private async Task<Guid?> NormalizeManagerBaseAssetHolderId(Guid? managerId)
    {
        if (!managerId.HasValue)
            return null;

        var id = managerId.Value;

        // Already a BaseAssetHolderId for an active poker manager.
        var existsAsBaseAssetHolderId = await _context.PokerManagers
            .AsNoTracking()
            .AnyAsync(m => !m.DeletedAt.HasValue && m.BaseAssetHolderId == id);
        if (existsAsBaseAssetHolderId)
            return id;

        // Provided as PokerManager.Id; normalize to BaseAssetHolderId.
        var mappedBaseAssetHolderId = await _context.PokerManagers
            .AsNoTracking()
            .Where(m => !m.DeletedAt.HasValue && m.Id == id)
            .Select(m => (Guid?)m.BaseAssetHolderId)
            .FirstOrDefaultAsync();

        if (mappedBaseAssetHolderId.HasValue)
        {
            _logger.LogInformation(
                "Normalized managerId from PokerManager.Id {ManagerEntityId} to BaseAssetHolderId {BaseAssetHolderId}",
                id, mappedBaseAssetHolderId.Value);
            return mappedBaseAssetHolderId.Value;
        }

        _logger.LogWarning(
            "ManagerId {ManagerId} did not match an active PokerManager as BaseAssetHolderId or PokerManager.Id. Profit result will be zero for manager-scoped calculations.",
            id);
        return id;
    }
}
