using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SFManagement.Application.DTOs.Finance;
using SFManagement.Application.Services.Validation;
using SFManagement.Domain.Common;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Exceptions;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Finance;

public class AvgRateService : IAvgRateService
{
    private readonly DataContext _context;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AvgRateService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan ManagerWalletCacheDuration = TimeSpan.FromMinutes(10);
    private const string ManagerWalletsCacheKeyPrefix = "avgrate.manager-wallet-ids:";
    private static readonly TimeSpan InitialBalanceCacheDuration = TimeSpan.FromMinutes(10);
    private const string InitialBalanceCacheKeyPrefix = "avgrate.initial-balance:";
    private static readonly TimeSpan FirstMonthCacheDuration = TimeSpan.FromMinutes(10);
    private const string FirstMonthCacheKeyPrefix = "avgrate.first-month:";
    
    public AvgRateService(
        DataContext context, 
        IMemoryCache cache,
        ILogger<AvgRateService> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<decimal> GetAvgRateAtDate(Guid pokerManagerId, DateTime date)
    {
        if (!await RequiresAvgRateTracking(pokerManagerId))
        {
            // RakeOverrideCommission managers don't track weighted cost basis.
            // Their chips are quoted in BRL (1 chip = 1 BRL), so AvgRate = 1.
            // If a future manager type works with non-BRL chips, this fallback
            // must be replaced with actual AvgRate tracking or a configurable rate.
            var isActiveManager = await _context.PokerManagers
                .AsNoTracking()
                .AnyAsync(pm => pm.BaseAssetHolderId == pokerManagerId && !pm.DeletedAt.HasValue);

            if (isActiveManager)
            {
                _logger.LogDebug("AvgRate not tracked for {ManagerId}; returning 1 (BRL-quoted chips)", pokerManagerId);
                return 1;
            }

            _logger.LogDebug("AvgRate not required for {ManagerId} (not a poker manager)", pokerManagerId);
            return 0;
        }

        if (IsCurrentMonth(date.Year, date.Month))
        {
            _logger.LogDebug("Calculating AvgRate dynamically for current month: {Year}-{Month}", date.Year, date.Month);
            return await CalculateAvgRateUpToDate(pokerManagerId, date);
        }
        
        var snapshot = await GetAvgRateForMonth(pokerManagerId, date.Year, date.Month);
        return snapshot.AvgRate;
    }
    
    public async Task<AvgRateSnapshot> GetAvgRateForMonth(Guid pokerManagerId, int year, int month)
    {
        if (!await RequiresAvgRateTracking(pokerManagerId))
        {
            _logger.LogDebug("AvgRate not required for {ManagerId} {Year}-{Month}", pokerManagerId, year, month);
            return new AvgRateSnapshot
            {
                PokerManagerId = pokerManagerId,
                Year = year,
                Month = month,
                AvgRate = 0,
                TotalChips = 0,
                TotalCost = 0,
                CalculatedAt = DateTime.UtcNow
            };
        }

        var cacheKey = GetCacheKey(pokerManagerId, year, month);
        
        if (!IsCurrentMonth(year, month) &&
            _cache.TryGetValue(cacheKey, out AvgRateSnapshot? cached) &&
            cached != null)
        {
            _logger.LogDebug("AvgRate cache hit for {ManagerId} {Year}-{Month}", pokerManagerId, year, month);
            return cached;
        }
        
        _logger.LogDebug("Calculating AvgRate for {ManagerId} {Year}-{Month}", pokerManagerId, year, month);
        
        // IMPORTANT: Use iterative calculation to avoid stack overflow.
        // Instead of recursively calling GetAvgRateForMonth -> CalculateMonthlySnapshot -> GetAvgRateForMonth,
        // we find the starting point and calculate forward month-by-month, caching each result.
        var snapshot = await CalculateMonthlySnapshotIterative(pokerManagerId, year, month);
        
        if (!IsCurrentMonth(year, month))
        {
            _cache.Set(cacheKey, snapshot, CacheDuration);
            _logger.LogDebug("AvgRate cached for {ManagerId} {Year}-{Month}: AvgRate={AvgRate}", 
                pokerManagerId, year, month, snapshot.AvgRate);
        }
        
        return snapshot;
    }
    
    public async Task InvalidateFromDate(Guid pokerManagerId, DateTime fromDate)
    {
        var currentDate = DateTime.UtcNow;
        var monthToInvalidate = new DateTime(fromDate.Year, fromDate.Month, 1);
        var currentMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        
        var invalidatedCount = 0;
        while (monthToInvalidate <= currentMonth)
        {
            var cacheKey = GetCacheKey(pokerManagerId, monthToInvalidate.Year, monthToInvalidate.Month);
            _cache.Remove(cacheKey);
            invalidatedCount++;
            monthToInvalidate = monthToInvalidate.AddMonths(1);
        }
        
        _logger.LogInformation(
            "Invalidated {Count} months of AvgRate cache for {ManagerId} from {FromDate:yyyy-MM}",
            invalidatedCount, pokerManagerId, fromDate);
        
        await Task.CompletedTask;
    }

    public void InvalidateManagerWalletCache(Guid pokerManagerId)
    {
        var cacheKey = $"{ManagerWalletsCacheKeyPrefix}{pokerManagerId}";
        _cache.Remove(cacheKey);
    }

    public async Task<bool> RequiresAvgRateTracking(Guid assetHolderId)
    {
        return await _context.PokerManagers
            .AsNoTracking()
            .AnyAsync(pm => pm.BaseAssetHolderId == assetHolderId
                && pm.ManagerProfitType == ManagerProfitType.Spread
                && !pm.DeletedAt.HasValue);
    }

    public async Task<AvgRateCalculationMode> GetCalculationMode(Guid assetHolderId, AssetGroup assetGroup)
    {
        var hasGroupInitialBalance = await _context.InitialBalances
            .AsNoTracking()
            .AnyAsync(ib => ib.BaseAssetHolderId == assetHolderId
                && ib.AssetGroup == assetGroup
                && ib.AssetType == AssetType.None
                && !ib.DeletedAt.HasValue);

        if (hasGroupInitialBalance)
        {
            return AvgRateCalculationMode.Consolidated;
        }

        var hasAssetTypeInitialBalance = await _context.InitialBalances
            .AsNoTracking()
            .AnyAsync(ib => ib.BaseAssetHolderId == assetHolderId
                && ib.AssetType != AssetType.None
                && WalletIdentifierValidationService.GetAssetGroupForAssetType(ib.AssetType) == assetGroup
                && !ib.DeletedAt.HasValue);

        if (hasAssetTypeInitialBalance)
        {
            return AvgRateCalculationMode.PerAssetType;
        }

        return AvgRateCalculationMode.PerAssetType;
    }
    
    /// <summary>
    /// Iteratively calculates AvgRate for a target month by finding the starting point
    /// and calculating forward month-by-month. This approach avoids stack overflow that
    /// would occur with deep recursion when many months need to be calculated.
    /// 
    /// Algorithm:
    /// 1. Walk backwards from target month to find the earliest uncached month
    /// 2. Calculate from that month forward, caching each result before proceeding
    /// 3. Return the final snapshot for the target month
    /// </summary>
    private async Task<AvgRateSnapshot> CalculateMonthlySnapshotIterative(Guid pokerManagerId, int year, int month)
    {
        // Step 1: Find the starting point - either a cached month or the first month
        var monthsToCalculate = new Stack<(int Year, int Month)>();
        var currentYear = year;
        var currentMonth = month;
        AvgRateSnapshot? startingSnapshot = null;
        var financeStartMonth = new DateTime(
            SystemImplementation.FinanceDataStartDateUtc.Year,
            SystemImplementation.FinanceDataStartDateUtc.Month,
            1);
        
        // Walk backwards to find cached data or the first month
        while (true)
        {
            var currentMonthStart = new DateTime(currentYear, currentMonth, 1);
            if (currentMonthStart < financeStartMonth)
            {
                _logger.LogInformation(
                    "AvgRate backward walk reached system implementation start ({FinanceStart:yyyy-MM}) for {ManagerId}. Stopping lookback.",
                    financeStartMonth, pokerManagerId);

                startingSnapshot = new AvgRateSnapshot
                {
                    PokerManagerId = pokerManagerId,
                    TotalChips = 0,
                    TotalCost = 0,
                    AvgRate = 0
                };
                break;
            }

            // Check if this month is already cached
            var cacheKey = GetCacheKey(pokerManagerId, currentYear, currentMonth);
            if (!IsCurrentMonth(currentYear, currentMonth) &&
                _cache.TryGetValue(cacheKey, out AvgRateSnapshot? cached) &&
                cached != null)
            {
                startingSnapshot = cached;
                _logger.LogDebug("Found cached snapshot at {Year}-{Month} for backward walk", currentYear, currentMonth);
                break;
            }
            
            // Check if this is the first calculable month
            if (await IsFirstCalculatedMonth(pokerManagerId, currentYear, currentMonth))
            {
                // This is the first month - we'll start from initial balance
                monthsToCalculate.Push((currentYear, currentMonth));
                
                var initialBalance = await GetInitialBalance(pokerManagerId, AssetType.None);
                var initialChips = initialBalance?.Balance ?? 0;
                var initialAvgRate = initialBalance?.ConversionRate > 0
                    ? initialBalance.ConversionRate.Value
                    : 0;
                
                startingSnapshot = new AvgRateSnapshot
                {
                    PokerManagerId = pokerManagerId,
                    TotalChips = initialChips,
                    TotalCost = initialChips * initialAvgRate,
                    AvgRate = initialAvgRate
                };
                
                _logger.LogDebug("Starting from InitialBalance for {ManagerId}: Chips={Chips}, AvgRate={AvgRate}",
                    pokerManagerId, startingSnapshot.TotalChips, startingSnapshot.AvgRate);
                break;
            }
            
            // Queue this month for calculation and move to previous month
            monthsToCalculate.Push((currentYear, currentMonth));
            
            if (currentMonth == 1)
            {
                currentYear--;
                currentMonth = 12;
            }
            else
            {
                currentMonth--;
            }
            
            // Safety check - don't go back more than 20 years
            if (year - currentYear > 20)
            {
                _logger.LogWarning("AvgRate calculation exceeded 20 year lookback for {ManagerId}, stopping", pokerManagerId);
                startingSnapshot = new AvgRateSnapshot
                {
                    PokerManagerId = pokerManagerId,
                    TotalChips = 0,
                    TotalCost = 0,
                    AvgRate = 0
                };
                break;
            }
        }
        
        // Step 2: Calculate forward from starting point, caching each month
        var previousSnapshot = startingSnapshot!;
        var walletIds = await GetPokerAssetWalletIds(pokerManagerId);
        
        _logger.LogDebug("Calculating {Count} months forward for {ManagerId}", monthsToCalculate.Count, pokerManagerId);
        
        while (monthsToCalculate.Count > 0)
        {
            var (calcYear, calcMonth) = monthsToCalculate.Pop();
            
            // Calculate this month based on previous snapshot
            var snapshot = await CalculateSingleMonth(pokerManagerId, calcYear, calcMonth, previousSnapshot, walletIds);
            
            // Cache the result if it's not the current month
            if (!IsCurrentMonth(calcYear, calcMonth))
            {
                var cacheKey = GetCacheKey(pokerManagerId, calcYear, calcMonth);
                _cache.Set(cacheKey, snapshot, CacheDuration);
                _logger.LogDebug("Cached AvgRate for {ManagerId} {Year}-{Month}: AvgRate={AvgRate}", 
                    pokerManagerId, calcYear, calcMonth, snapshot.AvgRate);
            }
            
            previousSnapshot = snapshot;
        }
        
        return previousSnapshot;
    }
    
    /// <summary>
    /// Calculates a single month's snapshot given the previous month's snapshot.
    /// This is the non-recursive core calculation that processes transactions.
    /// </summary>
    private async Task<AvgRateSnapshot> CalculateSingleMonth(
        Guid pokerManagerId, 
        int year, 
        int month, 
        AvgRateSnapshot previousSnapshot,
        List<Guid> walletIds)
    {
        decimal totalChips = previousSnapshot.TotalChips;
        decimal totalCost = previousSnapshot.TotalCost;
        
        var transactions = await GetTransactionsForMonth(walletIds, year, month);
        
        foreach (var tx in OrderTransactionsForAvgRate(transactions, walletIds))
        {
            ApplyTransactionImpact(
                pokerManagerId,
                tx,
                walletIds,
                ref totalChips,
                ref totalCost,
                $"{year}-{month:D2}");
        }
        
        var avgRate = totalChips > 0 ? totalCost / totalChips : 0;
        
        return new AvgRateSnapshot
        {
            PokerManagerId = pokerManagerId,
            Year = year,
            Month = month,
            AvgRate = avgRate,
            TotalChips = totalChips,
            TotalCost = totalCost,
            CalculatedAt = DateTime.UtcNow
        };
    }
    
    // Keep for reference - this was the original recursive implementation that caused stack overflow
    [Obsolete("Use CalculateMonthlySnapshotIterative instead to avoid stack overflow")]
    private async Task<AvgRateSnapshot> CalculateMonthlySnapshot(Guid pokerManagerId, int year, int month)
    {
        AvgRateSnapshot previousSnapshot;
        
        if (await IsFirstCalculatedMonth(pokerManagerId, year, month))
        {
            var initialBalance = await GetInitialBalance(pokerManagerId, AssetType.None);
            
            var initialChips = initialBalance?.Balance ?? 0;
            var initialAvgRate = initialBalance?.ConversionRate > 0
                ? initialBalance.ConversionRate.Value
                : 0;
            
            previousSnapshot = new AvgRateSnapshot
            {
                PokerManagerId = pokerManagerId,
                TotalChips = initialChips,
                TotalCost = initialChips * initialAvgRate,
                AvgRate = initialAvgRate
            };
            
            _logger.LogDebug("Using initial values from InitialBalance for {ManagerId}: Chips={Chips}, AvgRate={AvgRate}",
                pokerManagerId, previousSnapshot.TotalChips, previousSnapshot.AvgRate);
        }
        else
        {
            var prevYear = month == 1 ? year - 1 : year;
            var prevMonth = month == 1 ? 12 : month - 1;
            previousSnapshot = await GetAvgRateForMonth(pokerManagerId, prevYear, prevMonth);
        }
        
        decimal totalChips = previousSnapshot.TotalChips;
        decimal totalCost = previousSnapshot.TotalCost;
        
        var walletIds = await GetPokerAssetWalletIds(pokerManagerId);
        var transactions = await GetTransactionsForMonth(walletIds, year, month);
        
        foreach (var tx in OrderTransactionsForAvgRate(transactions, walletIds))
        {
            ApplyTransactionImpact(
                pokerManagerId,
                tx,
                walletIds,
                ref totalChips,
                ref totalCost,
                $"{year}-{month:D2}");
        }
        
        var avgRate = totalChips > 0 ? totalCost / totalChips : 0;
        
        return new AvgRateSnapshot
        {
            PokerManagerId = pokerManagerId,
            Year = year,
            Month = month,
            AvgRate = avgRate,
            TotalChips = totalChips,
            TotalCost = totalCost,
            CalculatedAt = DateTime.UtcNow
        };
    }
    
    private async Task<decimal> CalculateAvgRateUpToDate(Guid pokerManagerId, DateTime upToDate)
    {
        var prevMonth = upToDate.AddMonths(-1);
        var previousSnapshot = await GetAvgRateForMonth(pokerManagerId, prevMonth.Year, prevMonth.Month);
        
        decimal totalChips = previousSnapshot.TotalChips;
        decimal totalCost = previousSnapshot.TotalCost;

        // If this is the manager's first calculable month, seed the running state from InitialBalance.
        // Without this, current-month dynamic calculation may ignore InitialBalance and overstate spread profit.
        if (await IsFirstCalculatedMonth(pokerManagerId, upToDate.Year, upToDate.Month))
        {
            var initialBalance = await GetInitialBalance(pokerManagerId, AssetType.None);
            if (initialBalance != null)
            {
                var initialChips = initialBalance.Balance;
                var initialAvgRate = initialBalance.ConversionRate > 0
                    ? initialBalance.ConversionRate.Value
                    : 0;

                totalChips = initialChips;
                totalCost = initialChips * initialAvgRate;
            }
        }
        
        var walletIds = await GetPokerAssetWalletIds(pokerManagerId);
        var monthStart = new DateTime(upToDate.Year, upToDate.Month, 1);
        
        var transactions = await _context.DigitalAssetTransactions
            .AsNoTracking()
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= monthStart
                && t.Date <= upToDate
                && (walletIds.Contains(t.SenderWalletIdentifierId) 
                    || walletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();
        
        foreach (var tx in OrderTransactionsForAvgRate(transactions, walletIds))
        {
            ApplyTransactionImpact(
                pokerManagerId,
                tx,
                walletIds,
                ref totalChips,
                ref totalCost,
                $"{upToDate:yyyy-MM}");
        }
        
        return totalChips > 0 ? totalCost / totalChips : 0;
    }
    
    private async Task<bool> IsFirstCalculatedMonth(Guid pokerManagerId, int year, int month)
    {
        var cacheKey = GetFirstMonthCacheKey(pokerManagerId, year, month);
        if (_cache.TryGetValue(cacheKey, out bool cachedResult))
        {
            return cachedResult;
        }

        var initialBalance = await GetInitialBalance(pokerManagerId);
        
        if (initialBalance?.CreatedAt != null)
        {
            var isFirstMonth = year == initialBalance.CreatedAt.Value.Year &&
                               month == initialBalance.CreatedAt.Value.Month;
            _cache.Set(cacheKey, isFirstMonth, FirstMonthCacheDuration);
            return isFirstMonth;
        }
        
        var walletIds = await GetPokerAssetWalletIds(pokerManagerId);
        if (!walletIds.Any())
        {
            _cache.Set(cacheKey, true, FirstMonthCacheDuration);
            return true;
        }
        
        var monthStart = new DateTime(year, month, 1);
        
        var hasEarlierTransactions = await _context.DigitalAssetTransactions
            .AsNoTracking()
            .AnyAsync(t => !t.DeletedAt.HasValue
                && t.Date < monthStart
                && (walletIds.Contains(t.SenderWalletIdentifierId)
                    || walletIds.Contains(t.ReceiverWalletIdentifierId)));
        
        var result = !hasEarlierTransactions;
        _cache.Set(cacheKey, result, FirstMonthCacheDuration);
        return result;
    }
    
    private async Task<List<Guid>> GetPokerAssetWalletIds(Guid pokerManagerId)
    {
        var cacheKey = $"{ManagerWalletsCacheKeyPrefix}{pokerManagerId}";

        if (_cache.TryGetValue(cacheKey, out List<Guid>? cachedWalletIds))
        {
            _logger.LogDebug("AvgRate wallet cache hit for {ManagerId}", pokerManagerId);
            return cachedWalletIds ?? new List<Guid>();
        }

        _logger.LogDebug("AvgRate wallet cache miss for {ManagerId}", pokerManagerId);

        var walletIds = await (
                from wallet in _context.WalletIdentifiers.AsNoTracking()
                join pool in _context.AssetPools.AsNoTracking()
                    on wallet.AssetPoolId equals pool.Id
                where !wallet.DeletedAt.HasValue
                      && !pool.DeletedAt.HasValue
                      && pool.BaseAssetHolderId == pokerManagerId
                      && pool.AssetGroup == AssetGroup.PokerAssets
                select wallet.Id
            )
            .ToListAsync();

        _cache.Set(cacheKey, walletIds, ManagerWalletCacheDuration);
        return walletIds;
    }
    
    private async Task<List<DigitalAssetTransaction>> 
        GetTransactionsForMonth(List<Guid> walletIds, int year, int month)
    {
        if (!walletIds.Any()) return [];
        
        var monthStart = new DateTime(year, month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        
        return await _context.DigitalAssetTransactions
            .AsNoTracking()
            .Where(t => !t.DeletedAt.HasValue
                && t.Date >= monthStart
                && t.Date <= monthEnd
                && (walletIds.Contains(t.SenderWalletIdentifierId)
                    || walletIds.Contains(t.ReceiverWalletIdentifierId)))
            .ToListAsync();
    }

    /// <summary>
    /// Orders transactions by day and processes receives before sends within each day
    /// to avoid registration-time inversions affecting weighted cost basis.
    /// </summary>
    private static IEnumerable<DigitalAssetTransaction> OrderTransactionsForAvgRate(
        IEnumerable<DigitalAssetTransaction> transactions,
        List<Guid> walletIds)
    {
        return transactions
            .OrderBy(t => t.Date.Date)
            .ThenBy(t => GetDirectionRank(t, walletIds))
            .ThenBy(t => t.CreatedAt);
    }

    private static int GetDirectionRank(DigitalAssetTransaction tx, List<Guid> walletIds)
    {
        var isReceiving = walletIds.Contains(tx.ReceiverWalletIdentifierId);
        var isSending = walletIds.Contains(tx.SenderWalletIdentifierId);

        if (isReceiving && !isSending) return 0; // receives first
        if (isSending && !isReceiving) return 1; // sends second
        return 2; // internal/self/unknown last
    }

    private void ApplyTransactionImpact(
        Guid pokerManagerId,
        DigitalAssetTransaction tx,
        List<Guid> walletIds,
        ref decimal totalChips,
        ref decimal totalCost,
        string periodLabel)
    {
        var isReceiving = walletIds.Contains(tx.ReceiverWalletIdentifierId);
        var isSending = walletIds.Contains(tx.SenderWalletIdentifierId);

        if (isReceiving && isSending)
        {
            return;
        }

        if (isReceiving)
        {
            var currentAvgRate = totalChips > 0 ? totalCost / totalChips : 0;
            var receivePrice = tx.ConversionRate ?? currentAvgRate;
            totalChips += tx.AssetAmount;
            totalCost += tx.AssetAmount * receivePrice;
            return;
        }

        if (!isSending)
        {
            return;
        }

        if (totalChips <= 0)
        {
            LogAndThrowNegativeState(
                pokerManagerId,
                tx,
                periodLabel,
                totalChips,
                totalCost,
                "send attempted with no available chips");
        }

        var proportion = tx.AssetAmount / totalChips;
        totalCost -= totalCost * proportion;
        totalChips -= tx.AssetAmount;

        if (totalChips < 0 || totalCost < 0)
        {
            LogAndThrowNegativeState(
                pokerManagerId,
                tx,
                periodLabel,
                totalChips,
                totalCost,
                "resulting state became negative after send");
        }
    }

    private void LogAndThrowNegativeState(
        Guid pokerManagerId,
        DigitalAssetTransaction tx,
        string periodLabel,
        decimal totalChips,
        decimal totalCost,
        string reason)
    {
        _logger.LogError(
            "CRITICAL AvgRate state for manager {ManagerId} at {Period}. TxId={TransactionId}, TxDate={TxDate:yyyy-MM-dd}, Amount={Amount}, Chips={Chips}, Cost={Cost}, Reason={Reason}.",
            pokerManagerId, periodLabel, tx.Id, tx.Date, tx.AssetAmount, totalChips, totalCost, reason);

        throw new BusinessException(
            $"Invalid AvgRate state for manager {pokerManagerId} at {periodLabel}. " +
            $"Transaction {tx.Id} produced negative/unavailable chips ({reason}).");
    }
    
    private static string GetCacheKey(Guid managerId, int year, int month)
        => $"AvgRate:{managerId}:{year}:{month:D2}";

    private static string GetInitialBalanceCacheKey(Guid managerId)
        => $"{InitialBalanceCacheKeyPrefix}{managerId}";

    private static string GetFirstMonthCacheKey(Guid managerId, int year, int month)
        => $"{FirstMonthCacheKeyPrefix}{managerId}:{year}:{month:D2}";

    private async Task<InitialBalance?> GetInitialBalance(Guid pokerManagerId, AssetType? assetType = null)
    {
        var cacheKey = GetInitialBalanceCacheKey(pokerManagerId);
        if (_cache.TryGetValue(cacheKey, out InitialBalance? cached))
        {
            return cached;
        }

        var query = _context.InitialBalances
            .AsNoTracking()
            .Where(ib =>
                ib.BaseAssetHolderId == pokerManagerId &&
                ib.AssetGroup == AssetGroup.PokerAssets &&
                !ib.DeletedAt.HasValue);

        if (assetType.HasValue)
        {
            query = query.Where(ib => ib.AssetType == assetType.Value);
        }

        var initialBalance = await query.FirstOrDefaultAsync();
        _cache.Set(cacheKey, initialBalance, InitialBalanceCacheDuration);
        return initialBalance;
    }
    
    private static bool IsCurrentMonth(int year, int month)
    {
        var now = DateTime.UtcNow;
        return year == now.Year && month == now.Month;
    }
}
