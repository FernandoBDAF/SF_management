using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Models.AssetInfrastructure;
using SFManagement.Enums;
using SFManagement.ViewModels;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.Services;

public class AssetPoolService(DataContext context, IHttpContextAccessor httpContextAccessor)
    : BaseService<AssetPool>(context, httpContextAccessor)
{
    public override async Task<AssetPool> Add(AssetPool obj)
    {
        // Enhanced validation for both asset holder and company pools
        if (obj.BaseAssetHolderId.HasValue)
        {
            // Validate BaseAssetHolder exists
            var assetHolderExists = await context.BaseAssetHolders
                .AnyAsync(bah => bah.Id == obj.BaseAssetHolderId.Value && !bah.DeletedAt.HasValue);
            
            if (!assetHolderExists)
            {
                throw new InvalidOperationException($"BaseAssetHolder {obj.BaseAssetHolderId.Value} does not exist");
            }
            
            // Check if BaseAssetHolder already has an AssetPool for this AssetType
            var existingAssetHolderPool = await context.AssetPools
                .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == obj.BaseAssetHolderId && 
                                         aw.AssetGroup == obj.AssetGroup && 
                                         !aw.DeletedAt.HasValue);
            
            if (existingAssetHolderPool != null)
            {
                throw new InvalidOperationException($"BaseAssetHolder {obj.BaseAssetHolderId} already has an AssetPool for {obj.AssetGroup}");
            }
        }
        else
        {
            // Company pool validation - only one company pool per AssetType allowed
            var existingCompanyPool = await context.AssetPools
                .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == null && 
                                         aw.AssetGroup == obj.AssetGroup && 
                                         !aw.DeletedAt.HasValue);
            
            if (existingCompanyPool != null)
            {
                throw new InvalidOperationException($"Company already has an AssetPool for {obj.AssetGroup}. Company pool ID: {existingCompanyPool.Id}");
            }
        }
        
        return await base.Add(obj);
    }

    public async Task<List<AssetPool>> GetAssetPools(Guid assetHolderId)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Where(aw => aw.BaseAssetHolderId == assetHolderId && !aw.DeletedAt.HasValue)
            .ToListAsync();
    }

    public async Task<AssetPool?> GetAssetPoolByGroup(AssetGroup assetGroup)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Where(aw => aw.AssetGroup == assetGroup && !aw.DeletedAt.HasValue)
            .FirstOrDefaultAsync();
    }

    public override async Task<AssetPool?> Get(Guid id)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.Id == id && !aw.DeletedAt.HasValue);
    }

    public async Task<AssetPool?> GetCompanyAssetPoolByGroup(AssetGroup assetGroup)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == null && 
                                     aw.AssetGroup == assetGroup && 
                                     !aw.DeletedAt.HasValue);
    }

    public async Task<AssetPool?> GetByBaseAssetHolderAndType(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        return await context.AssetPools
            .Include(aw => aw.BaseAssetHolder)
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == baseAssetHolderId && 
                                     aw.AssetGroup == assetGroup && 
                                     !aw.DeletedAt.HasValue);
    }

    /// <summary>
    /// Gets company-owned asset pools (where BaseAssetHolderId is null)
    /// </summary>
    public async Task<List<AssetPool>> GetCompanyAssetPools()
    {
        return await context.AssetPools
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Where(aw => aw.BaseAssetHolderId == null && !aw.DeletedAt.HasValue)
            .ToListAsync();
    }

    /// <summary>
    /// Gets company pool for specific asset group
    /// </summary>
    public async Task<AssetPool?> GetCompanyAssetPoolByType(AssetGroup assetGroup)
    {
        return await context.AssetPools
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.BaseAssetHolderId == null && 
                                     aw.AssetGroup == assetGroup && 
                                     !aw.DeletedAt.HasValue);
    }

    /// <summary>
    /// Validates if an AssetPool belongs to the company
    /// </summary>
    public async Task<bool> IsCompanyPool(Guid assetPoolId)
    {
        return await context.AssetPools
            .AnyAsync(aw => aw.Id == assetPoolId && 
                          aw.BaseAssetHolderId == null && 
                          !aw.DeletedAt.HasValue);
    }

    // Get wallets with their identifiers grouped by wallet type
    public async Task<Dictionary<AssetGroup, List<WalletIdentifier>>> GetWalletIdentifiersByType(Guid AssetPoolId)
    {
        var assetPool = await context.AssetPools
            .Include(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(aw => aw.Id == AssetPoolId && !aw.DeletedAt.HasValue);

        if (assetPool == null)
            return new Dictionary<AssetGroup, List<WalletIdentifier>>();

        return assetPool.WalletIdentifiers
            .GroupBy(wi => wi.AssetGroup)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    // Get balance summary for an asset wallet
    public async Task<decimal> GetAssetPoolBalance(Guid AssetPoolId)
    {
        var walletIdentifiers = await context.WalletIdentifiers
            .Where(wi => wi.AssetPoolId == AssetPoolId && !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToListAsync();

        if (!walletIdentifiers.Any())
            return 0;

        // Calculate balance from all transaction types
        var fiatBalance = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                        (walletIdentifiers.Contains(ft.SenderWalletIdentifierId) || 
                         walletIdentifiers.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.ReceiverWalletIdentifier)
            .Include(ft => ft.SenderWalletIdentifier)
            .SumAsync(ft => walletIdentifiers.Contains(ft.ReceiverWalletIdentifierId) ? 
            ft.AssetAmount * (ft.ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY ? -1 : 1) : 
            -ft.AssetAmount * (ft.ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY ? -1 : 1)
            );

        var digitalBalance = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                        (walletIdentifiers.Contains(dt.SenderWalletIdentifierId) || 
                         walletIdentifiers.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.ReceiverWalletIdentifier)
            .Include(dt => dt.SenderWalletIdentifier)
            .SumAsync(dt => walletIdentifiers.Contains(dt.ReceiverWalletIdentifierId) ? 
            dt.AssetAmount * (dt.ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY ? -1 : 1) : 
            -dt.AssetAmount * (dt.ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY ? -1 : 1)
            );

        var settlementBalance = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                        (walletIdentifiers.Contains(st.SenderWalletIdentifierId) || 
                         walletIdentifiers.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.ReceiverWalletIdentifier)
            .Include(st => st.SenderWalletIdentifier)
            .SumAsync(st => walletIdentifiers.Contains(st.ReceiverWalletIdentifierId) ? 
            st.AssetAmount * (st.ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY ? -1 : 1) : 
            -st.AssetAmount * (st.ReceiverWalletIdentifier.AccountClassification == AccountClassification.LIABILITY ? -1 : 1)
            );

        return fiatBalance + digitalBalance + settlementBalance;
    }

    /// <summary>
    /// Creates a company-owned asset pool with validation and business logic
    /// </summary>
    public async Task<AssetPool> CreateCompanyAssetPool(AssetGroup assetGroup, string? description = null, string? businessJustification = null)
    {
        var assetPool = new AssetPool
        {
            BaseAssetHolderId = null, // Explicitly company-owned
            AssetGroup = assetGroup
        };
        
        // Use existing validation through Add method
        var createdPool = await Add(assetPool);
        
        // Log company pool creation for audit purposes
        // Note: You might want to add a logging service here
        
        return createdPool;
    }

    /// <summary>
    /// Gets detailed company asset pool information including metrics
    /// </summary>
    public async Task<AssetPool?> GetCompanyAssetPoolWithMetrics(AssetGroup assetGroup)
    {
        var pool = await context.AssetPools
            .Include(ap => ap.BaseAssetHolder)
            .Include(ap => ap.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .FirstOrDefaultAsync(ap => ap.BaseAssetHolderId == null && 
                                     ap.AssetGroup == assetGroup && 
                                     !ap.DeletedAt.HasValue);
        
        return pool;
    }

    /// <summary>
    /// Gets company asset pool summary with activity metrics
    /// </summary>
    public async Task<CompanyAssetPoolSummaryResponse> GetCompanyAssetPoolSummary()
    {
        var companyPools = await GetCompanyAssetPools();
        
        var summary = new CompanyAssetPoolSummaryResponse
        {
            TotalPools = companyPools.Count
        };
        
        // Calculate balances and metrics for each asset group
        foreach (var pool in companyPools)
        {
            var balance = await GetAssetPoolBalance(pool.Id);
            var transactionCount = await GetAssetPoolTransactionCount(pool.Id);
            var lastTransactionDate = await GetLastTransactionDate(pool.Id);
            
            summary.AssetGroupBalances.Add(new CompanyAssetGroupBalance
            {
                AssetGroup = pool.AssetGroup,
                AssetGroupName = pool.AssetGroup.ToString(),
                Balance = balance,
                WalletIdentifierCount = pool.WalletIdentifiers.Count,
                TransactionCount = transactionCount,
                LastTransactionDate = lastTransactionDate
            });
            
            summary.TotalBalance += balance;
        }
        
        // Calculate recent activity (last 30 days)
        summary.RecentActivity = await GetCompanyPoolRecentActivity(companyPools);
        
        return summary;
    }

    /// <summary>
    /// Gets comprehensive analytics for company asset pools by period
    /// </summary>
    public async Task<CompanyAssetPoolAnalyticsResponse> GetCompanyAssetPoolAnalytics(
        int year, 
        int? month = null, 
        bool includeTransactions = true, 
        int transactionLimit = 100)
    {
        var period = CreateAnalyticsPeriod(year, month);
        var companyPools = await GetCompanyAssetPools();
        
        var response = new CompanyAssetPoolAnalyticsResponse
        {
            Period = period
        };

        var summary = new CompanyAnalyticsSummary();
        var assetPoolData = new List<CompanyAssetPoolPeriodData>();

        foreach (var pool in companyPools)
        {
            var poolData = await GetAssetPoolPeriodData(pool, period, includeTransactions, transactionLimit);
            assetPoolData.Add(poolData);

            // Aggregate summary data
            summary.ActivePoolsCount++;
            summary.TotalStartingBalance += poolData.StartingBalance;
            summary.TotalEndingBalance += poolData.EndingBalance;
            summary.NetBalanceChange += poolData.NetBalanceChange;
            summary.TotalTransactionCount += poolData.TransactionCount;
            summary.TotalTransactionVolume += poolData.TotalTransactionVolume;
            
            if (poolData.LargestTransaction > summary.LargestTransaction)
                summary.LargestTransaction = poolData.LargestTransaction;
        }

        // Calculate averages and most active asset group
        if (summary.TotalTransactionCount > 0)
        {
            summary.AverageTransactionAmount = summary.TotalTransactionVolume / summary.TotalTransactionCount;
        }

        summary.MostActiveAssetGroup = assetPoolData
            .OrderByDescending(p => p.TransactionCount)
            .FirstOrDefault()?.AssetGroup;

        response.Summary = summary;
        response.AssetPoolData = assetPoolData;

        return response;
    }

    /// <summary>
    /// Creates period information for analytics
    /// </summary>
    private static AnalyticsPeriod CreateAnalyticsPeriod(int year, int? month)
    {
        DateTime startDate, endDate;
        string periodName;

        if (month.HasValue)
        {
            startDate = new DateTime(year, month.Value, 1);
            endDate = startDate.AddMonths(1).AddDays(-1);
            periodName = $"{startDate:MMMM yyyy}";
        }
        else
        {
            startDate = new DateTime(year, 1, 1);
            endDate = new DateTime(year, 12, 31);
            periodName = year.ToString();
        }

        return new AnalyticsPeriod
        {
            Year = year,
            Month = month,
            PeriodName = periodName,
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = (endDate - startDate).Days + 1
        };
    }

    /// <summary>
    /// Gets detailed period data for a specific asset pool
    /// </summary>
    private async Task<CompanyAssetPoolPeriodData> GetAssetPoolPeriodData(
        AssetPool pool, 
        AnalyticsPeriod period, 
        bool includeTransactions, 
        int transactionLimit)
    {
        var walletIdentifierIds = pool.WalletIdentifiers
            .Where(wi => !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToList();

        var poolData = new CompanyAssetPoolPeriodData
        {
            AssetPoolId = pool.Id,
            AssetGroup = pool.AssetGroup,
            AssetGroupName = pool.AssetGroup.ToString(),
            WalletIdentifierCount = walletIdentifierIds.Count
        };

        if (!walletIdentifierIds.Any())
        {
            return poolData; // Return empty data if no wallet identifiers
        }

        // Get transactions for the period
        var (fiatTransactions, digitalTransactions, settlementTransactions) = 
            await GetTransactionsForPeriod(walletIdentifierIds, period);

        // Calculate balances
        poolData.StartingBalance = await GetBalanceAtDate(walletIdentifierIds, period.StartDate.AddDays(-1));
        poolData.EndingBalance = await GetBalanceAtDate(walletIdentifierIds, period.EndDate);
        poolData.NetBalanceChange = poolData.EndingBalance - poolData.StartingBalance;

        // Calculate transaction metrics
        var allTransactions = new List<(decimal amount, DateTime date, string type, Guid id)>();
        
        foreach (var ft in fiatTransactions)
        {
            var amount = walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId) ? ft.AssetAmount : -ft.AssetAmount;
            allTransactions.Add((Math.Abs(ft.AssetAmount), ft.Date, "Fiat", ft.Id));
            poolData.TotalTransactionVolume += Math.Abs(ft.AssetAmount);
        }
        
        foreach (var dt in digitalTransactions)
        {
            var amount = walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId) ? dt.AssetAmount : -dt.AssetAmount;
            allTransactions.Add((Math.Abs(dt.AssetAmount), dt.Date, "Digital", dt.Id));
            poolData.TotalTransactionVolume += Math.Abs(dt.AssetAmount);
        }
        
        foreach (var st in settlementTransactions)
        {
            var amount = walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId) ? st.AssetAmount : -st.AssetAmount;
            allTransactions.Add((Math.Abs(st.AssetAmount), st.Date, "Settlement", st.Id));
            poolData.TotalTransactionVolume += Math.Abs(st.AssetAmount);
        }

        poolData.TransactionCount = allTransactions.Count;
        
        if (poolData.TransactionCount > 0)
        {
            poolData.AverageTransactionAmount = poolData.TotalTransactionVolume / poolData.TransactionCount;
            poolData.LargestTransaction = allTransactions.Max(t => t.amount);
        }

        // Transaction breakdown by type
        poolData.TransactionBreakdown = new TransactionTypeBreakdown
        {
            FiatTransactions = CreateTransactionTypeSummary(fiatTransactions.Select(ft => (decimal)ft.AssetAmount)),
            DigitalTransactions = CreateTransactionTypeSummary(digitalTransactions.Select(dt => (decimal)dt.AssetAmount)),
            SettlementTransactions = CreateTransactionTypeSummary(settlementTransactions.Select(st => (decimal)st.AssetAmount))
        };

        // Include detailed transactions if requested
        if (includeTransactions)
        {
            poolData.Transactions = await GetDetailedTransactionSummaries(
                fiatTransactions, digitalTransactions, settlementTransactions, 
                walletIdentifierIds, transactionLimit);
        }

        return poolData;
    }

    /// <summary>
    /// Gets transactions for a specific period
    /// </summary>
    private async Task<(List<dynamic> fiat, List<dynamic> digital, List<dynamic> settlement)> 
        GetTransactionsForPeriod(List<Guid> walletIdentifierIds, AnalyticsPeriod period)
    {
        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                        ft.Date >= period.StartDate && 
                        ft.Date <= period.EndDate &&
                        (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Select(ft => new { 
                ft.Id, 
                ft.AssetAmount, 
                ft.Date, 
                ft.SenderWalletIdentifierId, 
                ft.ReceiverWalletIdentifierId,
                ft.Description,
                Category = ft.Category != null ? ft.Category.Description : null
            })
            .ToListAsync();

        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                        dt.Date >= period.StartDate && 
                        dt.Date <= period.EndDate &&
                        (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Select(dt => new { 
                dt.Id, 
                dt.AssetAmount, 
                dt.Date, 
                dt.SenderWalletIdentifierId, 
                dt.ReceiverWalletIdentifierId,
                dt.Description,
                Category = dt.Category != null ? dt.Category.Description : null
            })
            .ToListAsync();

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                        st.Date >= period.StartDate && 
                        st.Date <= period.EndDate &&
                        (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Select(st => new { 
                st.Id, 
                st.AssetAmount, 
                st.Date, 
                st.SenderWalletIdentifierId, 
                st.ReceiverWalletIdentifierId,
                st.Description,
                Category = st.Category != null ? st.Category.Description : null
            })
            .ToListAsync();

        return (fiatTransactions.Cast<dynamic>().ToList(), 
                digitalTransactions.Cast<dynamic>().ToList(), 
                settlementTransactions.Cast<dynamic>().ToList());
    }

    /// <summary>
    /// Gets balance at a specific date
    /// </summary>
    private async Task<decimal> GetBalanceAtDate(List<Guid> walletIdentifierIds, DateTime date)
    {
        if (!walletIdentifierIds.Any())
            return 0;

        var fiatBalance = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                        ft.Date <= date &&
                        (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .SumAsync(ft => walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId) ? ft.AssetAmount : -ft.AssetAmount);

        var digitalBalance = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                        dt.Date <= date &&
                        (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .SumAsync(dt => walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId) ? dt.AssetAmount : -dt.AssetAmount);

        var settlementBalance = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                        st.Date <= date &&
                        (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .SumAsync(st => walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId) ? st.AssetAmount : -st.AssetAmount);

        return fiatBalance + digitalBalance + settlementBalance;
    }

    /// <summary>
    /// Creates transaction type summary from amounts
    /// </summary>
    private static TransactionTypeSummary CreateTransactionTypeSummary(IEnumerable<decimal> amounts)
    {
        var amountsList = amounts.ToList();
        if (!amountsList.Any())
            return new TransactionTypeSummary();

        return new TransactionTypeSummary
        {
            Count = amountsList.Count,
            TotalVolume = amountsList.Sum(Math.Abs),
            AverageAmount = amountsList.Average(Math.Abs),
            LargestAmount = amountsList.Max(Math.Abs)
        };
    }

    /// <summary>
    /// Gets detailed transaction summaries
    /// </summary>
    private async Task<List<CompanyPoolTransactionSummary>> GetDetailedTransactionSummaries(
        List<dynamic> fiatTransactions,
        List<dynamic> digitalTransactions,
        List<dynamic> settlementTransactions,
        List<Guid> walletIdentifierIds,
        int limit)
    {
        var summaries = new List<CompanyPoolTransactionSummary>();

        // Process fiat transactions
        foreach (var ft in fiatTransactions.Take(limit / 3))
        {
            var isIncoming = walletIdentifierIds.Contains((Guid)ft.ReceiverWalletIdentifierId);
            var counterpartyId = isIncoming ? ft.SenderWalletIdentifierId : ft.ReceiverWalletIdentifierId;
            var counterpartyName = await GetCounterpartyName(counterpartyId);

            summaries.Add(new CompanyPoolTransactionSummary
            {
                TransactionId = ft.Id,
                TransactionType = "Fiat",
                Date = ft.Date,
                Amount = ft.AssetAmount,
                Direction = isIncoming ? "Incoming" : "Outgoing",
                CounterpartyName = counterpartyName,
                Description = ft.Description,
                Category = ft.Category
            });
        }

        // Process digital transactions
        foreach (var dt in digitalTransactions.Take(limit / 3))
        {
            var isIncoming = walletIdentifierIds.Contains((Guid)dt.ReceiverWalletIdentifierId);
            var counterpartyId = isIncoming ? dt.SenderWalletIdentifierId : dt.ReceiverWalletIdentifierId;
            var counterpartyName = await GetCounterpartyName(counterpartyId);

            summaries.Add(new CompanyPoolTransactionSummary
            {
                TransactionId = dt.Id,
                TransactionType = "Digital",
                Date = dt.Date,
                Amount = dt.AssetAmount,
                Direction = isIncoming ? "Incoming" : "Outgoing",
                CounterpartyName = counterpartyName,
                Description = dt.Description,
                Category = dt.Category
            });
        }

        // Process settlement transactions
        foreach (var st in settlementTransactions.Take(limit / 3))
        {
            var isIncoming = walletIdentifierIds.Contains((Guid)st.ReceiverWalletIdentifierId);
            var counterpartyId = isIncoming ? st.SenderWalletIdentifierId : st.ReceiverWalletIdentifierId;
            var counterpartyName = await GetCounterpartyName(counterpartyId);

            summaries.Add(new CompanyPoolTransactionSummary
            {
                TransactionId = st.Id,
                TransactionType = "Settlement",
                Date = st.Date,
                Amount = st.AssetAmount,
                Direction = isIncoming ? "Incoming" : "Outgoing",
                CounterpartyName = counterpartyName,
                Description = st.Description,
                Category = st.Category
            });
        }

        return summaries.OrderByDescending(s => s.Date).Take(limit).ToList();
    }

    /// <summary>
    /// Gets counterparty name from wallet identifier
    /// </summary>
    private async Task<string?> GetCounterpartyName(Guid walletIdentifierId)
    {
        var walletIdentifier = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(ap => ap.BaseAssetHolder)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId);

        return walletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Company";
    }

    /// <summary>
    /// Gets transaction count for an asset pool
    /// </summary>
    private async Task<int> GetAssetPoolTransactionCount(Guid assetPoolId)
    {
        var walletIdentifierIds = await context.WalletIdentifiers
            .Where(wi => wi.AssetPoolId == assetPoolId && !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToListAsync();

        if (!walletIdentifierIds.Any())
            return 0;

        var fiatCount = await context.FiatAssetTransactions
            .CountAsync(ft => !ft.DeletedAt.HasValue && 
                            (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                             walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)));

        var digitalCount = await context.DigitalAssetTransactions
            .CountAsync(dt => !dt.DeletedAt.HasValue && 
                            (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                             walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)));

        var settlementCount = await context.SettlementTransactions
            .CountAsync(st => !st.DeletedAt.HasValue && 
                            (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                             walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)));

        return fiatCount + digitalCount + settlementCount;
    }

    /// <summary>
    /// Gets the last transaction date for an asset pool
    /// </summary>
    private async Task<DateTime?> GetLastTransactionDate(Guid assetPoolId)
    {
        var walletIdentifierIds = await context.WalletIdentifiers
            .Where(wi => wi.AssetPoolId == assetPoolId && !wi.DeletedAt.HasValue)
            .Select(wi => wi.Id)
            .ToListAsync();

        if (!walletIdentifierIds.Any())
            return null;

        var lastFiatDate = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                        (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .MaxAsync(ft => (DateTime?)ft.Date);

        var lastDigitalDate = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                        (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .MaxAsync(dt => (DateTime?)dt.Date);

        var lastSettlementDate = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                        (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                         walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .MaxAsync(st => (DateTime?)st.Date);

        var dates = new[] { lastFiatDate, lastDigitalDate, lastSettlementDate }
            .Where(d => d.HasValue)
            .Select(d => d!.Value);

        return dates.Any() ? dates.Max() : null;
    }

    /// <summary>
    /// Gets recent activity metrics for company pools
    /// </summary>
    private async Task<CompanyPoolActivity> GetCompanyPoolRecentActivity(List<AssetPool> companyPools)
    {
        var activity = new CompanyPoolActivity();
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        
        var allWalletIdentifierIds = companyPools
            .SelectMany(p => p.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Select(wi => wi.Id)
            .ToList();

        if (!allWalletIdentifierIds.Any())
            return activity;

        // Count recent transactions
        var recentFiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                        ft.Date >= thirtyDaysAgo &&
                        (allWalletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                         allWalletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .ToListAsync();

        var recentDigitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                        dt.Date >= thirtyDaysAgo &&
                        (allWalletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                         allWalletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .ToListAsync();

        var recentSettlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                        st.Date >= thirtyDaysAgo &&
                        (allWalletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                         allWalletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .ToListAsync();

        activity.TransactionsLast30Days = recentFiatTransactions.Count + 
                                        recentDigitalTransactions.Count + 
                                        recentSettlementTransactions.Count;

        // Calculate balance changes and largest transaction
        var allAmounts = new List<decimal>();
        
        foreach (var ft in recentFiatTransactions)
        {
            allAmounts.Add(ft.AssetAmount);
            if (allWalletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId))
                activity.BalanceChangeLast30Days += ft.AssetAmount;
            else
                activity.BalanceChangeLast30Days -= ft.AssetAmount;
        }
        
        foreach (var dt in recentDigitalTransactions)
        {
            allAmounts.Add(dt.AssetAmount);
            if (allWalletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId))
                activity.BalanceChangeLast30Days += dt.AssetAmount;
            else
                activity.BalanceChangeLast30Days -= dt.AssetAmount;
        }
        
        foreach (var st in recentSettlementTransactions)
        {
            allAmounts.Add(st.AssetAmount);
            if (allWalletIdentifierIds.Contains(st.ReceiverWalletIdentifierId))
                activity.BalanceChangeLast30Days += st.AssetAmount;
            else
                activity.BalanceChangeLast30Days -= st.AssetAmount;
        }

        activity.LargestTransactionAmount = allAmounts.Any() ? allAmounts.Max() : 0;

        // Find most active asset group
        var assetGroupActivity = companyPools
            .GroupBy(p => p.AssetGroup)
            .Select(g => new { AssetGroup = g.Key, Count = g.Sum(p => p.WalletIdentifiers.Count) })
            .OrderByDescending(x => x.Count)
            .FirstOrDefault();

        activity.MostActiveAssetGroup = assetGroupActivity?.AssetGroup;

        return activity;
    }
}