using Microsoft.EntityFrameworkCore;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Support;
using SFManagement.Models.Entities;

namespace SFManagement.Services;

public class InitialBalanceService : BaseService<InitialBalance>
{
    public InitialBalanceService(DataContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
    }

    /// <summary>
    /// Creates or updates an initial balance for a BaseAssetHolder and AssetType
    /// Only one active initial balance per BaseAssetHolder/AssetType combination is allowed
    /// </summary>
    public async Task<InitialBalance> SetInitialBalance(
        Guid baseAssetHolderId, 
        AssetType balanceUnit, 
        decimal balance,
        AssetType? balanceAs = null,
        decimal? conversionRate = null,
        string? description = null)
    {
        // Validate BaseAssetHolder exists
        var baseAssetHolder = await context.BaseAssetHolders
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (baseAssetHolder == null)
            throw new ArgumentException($"BaseAssetHolder not found: {baseAssetHolderId}");

        // Validate conversion parameters
        if (balanceAs.HasValue && balanceAs != balanceUnit && !conversionRate.HasValue)
            throw new ArgumentException("ConversionRate is required when BalanceAs differs from BalanceUnit");

        if (balanceAs.HasValue && balanceAs == balanceUnit && conversionRate.HasValue)
            throw new ArgumentException("ConversionRate should not be specified when BalanceAs equals BalanceUnit");

        // Check for existing active initial balance for this combination
        var existingBalance = await GetActiveInitialBalance(baseAssetHolderId, balanceUnit);
        
        if (existingBalance != null)
        {
            // Soft delete the existing balance
            existingBalance.DeletedAt = DateTime.UtcNow;
            existingBalance.UpdatedAt = DateTime.UtcNow;
        }

        // Create new initial balance
        var initialBalance = new InitialBalance
        {
            BaseAssetHolderId = baseAssetHolderId,
            BalanceUnit = balanceUnit,
            Balance = balance,
            BalanceAs = balanceAs,
            ConversionRate = conversionRate,
            Description = description
        };

        return await Add(initialBalance);
    }

    /// <summary>
    /// Gets the active initial balance for a BaseAssetHolder and AssetType
    /// </summary>
    public async Task<InitialBalance?> GetActiveInitialBalance(Guid baseAssetHolderId, AssetType assetType)
    {
        return await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .FirstOrDefaultAsync(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                                      ib.BalanceUnit == assetType && 
                                      !ib.DeletedAt.HasValue);
    }

    /// <summary>
    /// Gets all active initial balances for a BaseAssetHolder
    /// </summary>
    public async Task<List<InitialBalance>> GetInitialBalancesForAssetHolder(Guid baseAssetHolderId)
    {
        return await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .Where(ib => ib.BaseAssetHolderId == baseAssetHolderId && !ib.DeletedAt.HasValue)
            .OrderBy(ib => ib.BalanceUnit)
            .ToListAsync();
    }

    /// <summary>
    /// Gets initial balances grouped by AssetType for a BaseAssetHolder
    /// </summary>
    public async Task<Dictionary<AssetType, InitialBalance>> GetInitialBalancesByAssetType(Guid baseAssetHolderId)
    {
        var initialBalances = await GetInitialBalancesForAssetHolder(baseAssetHolderId);
        
        return initialBalances
            .Where(ib => ib.IsActive)
            .ToDictionary(ib => ib.BalanceUnit, ib => ib);
    }

    /// <summary>
    /// Gets the effective initial balance for a specific AssetType
    /// </summary>
    public async Task<decimal> GetEffectiveInitialBalance(Guid baseAssetHolderId, AssetType assetType)
    {
        var initialBalance = await context.InitialBalances
            .FirstOrDefaultAsync(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                                      ib.BalanceUnit == assetType && 
                                      !ib.DeletedAt.HasValue);
        
        return initialBalance?.EffectiveBalance ?? 0;
    }

    /// <summary>
    /// Gets initial balances for multiple AssetTypes for a BaseAssetHolder
    /// </summary>
    public async Task<Dictionary<AssetType, decimal>> GetEffectiveInitialBalances(
        Guid baseAssetHolderId, 
        IEnumerable<AssetType> assetTypes)
    {
        var result = new Dictionary<AssetType, decimal>();

        foreach (var assetType in assetTypes)
        {
            var balance = await GetEffectiveInitialBalance(baseAssetHolderId, assetType);
            result[assetType] = balance;
        }

        return result;
    }

    /// <summary>
    /// Calculates total balance including initial balances for a BaseAssetHolder by AssetType
    /// This integrates with transaction-based balance calculations
    /// </summary>
    public async Task<Dictionary<AssetType, decimal>> GetTotalBalancesWithInitial(Guid baseAssetHolderId)
    {
        // Get transaction-based balances (existing logic)
        var transactionBalances = await GetTransactionBalances(baseAssetHolderId);
        
        // Get initial balances
        var initialBalances = await GetInitialBalancesByAssetType(baseAssetHolderId);
        
        // Combine both
        var totalBalances = new Dictionary<AssetType, decimal>();
        
        // Add all asset types from both sources
        var allAssetTypes = transactionBalances.Keys
            .Union(initialBalances.Keys)
            .ToHashSet();
        
        foreach (var assetType in allAssetTypes)
        {
            var transactionBalance = transactionBalances.GetValueOrDefault(assetType, 0);
            var initialBalance = initialBalances.ContainsKey(assetType) 
                ? initialBalances[assetType].EffectiveBalance 
                : 0;
            
            totalBalances[assetType] = initialBalance + transactionBalance;
        }
        
        return totalBalances;
    }

    /// <summary>
    /// Removes an initial balance (soft delete)
    /// </summary>
    public async Task<bool> RemoveInitialBalance(Guid baseAssetHolderId, AssetType assetType)
    {
        var initialBalance = await GetActiveInitialBalance(baseAssetHolderId, assetType);
        
        if (initialBalance != null)
        {
            await Delete(initialBalance.Id);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Gets initial balance history for a BaseAssetHolder and AssetType
    /// Includes deleted/inactive balances for audit purposes
    /// </summary>
    public async Task<List<InitialBalance>> GetInitialBalanceHistory(Guid baseAssetHolderId, AssetType assetType)
    {
        return await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .Where(ib => ib.BaseAssetHolderId == baseAssetHolderId && ib.BalanceUnit == assetType)
            .OrderByDescending(ib => ib.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Validates initial balance data before creation/update
    /// </summary>
    public async Task<List<string>> ValidateInitialBalance(
        Guid baseAssetHolderId, 
        AssetType balanceUnit, 
        decimal balance,
        AssetType? balanceAs = null,
        decimal? conversionRate = null)
    {
        var errors = new List<string>();

        // Validate BaseAssetHolder exists
        var baseAssetHolderExists = await context.BaseAssetHolders
            .AnyAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (!baseAssetHolderExists)
            errors.Add("BaseAssetHolder not found or is deleted");

        // Validate balance amount
        if (balance < 0)
            errors.Add("Balance cannot be negative");

        // Validate conversion parameters
        if (balanceAs.HasValue && balanceAs != balanceUnit && !conversionRate.HasValue)
            errors.Add("ConversionRate is required when BalanceAs differs from BalanceUnit");

        if (balanceAs.HasValue && balanceAs == balanceUnit && conversionRate.HasValue)
            errors.Add("ConversionRate should not be specified when BalanceAs equals BalanceUnit");

        if (conversionRate.HasValue && conversionRate <= 0)
            errors.Add("ConversionRate must be positive");

        return errors;
    }

    /// <summary>
    /// Helper method to get transaction-based balances (replicates existing logic)
    /// </summary>
    private async Task<Dictionary<AssetType, decimal>> GetTransactionBalances(Guid baseAssetHolderId)
    {
        var balances = new Dictionary<AssetType, decimal>();

        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => wi.AssetPool.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
       
        // Get all transaction types
        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.SenderWalletIdentifier)
            .Include(dt => dt.ReceiverWalletIdentifier)
            .ToArrayAsync();

        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.SenderWalletIdentifier)
            .Include(ft => ft.ReceiverWalletIdentifier)
            .ToArrayAsync();

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.SenderWalletIdentifier)
            .Include(st => st.ReceiverWalletIdentifier)
            .ToArrayAsync();

        // Process all transaction types (same logic as existing GetBalancesByAssetType)
        ProcessTransactions(digitalTransactions, walletIdentifierIds, balances);
        ProcessTransactions(fiatTransactions, walletIdentifierIds, balances);
        ProcessTransactions(settlementTransactions, walletIdentifierIds, balances);

        return balances;
    }

    /// <summary>
    /// Helper method to process transactions and update balances
    /// </summary>
    private static void ProcessTransactions<T>(T[] transactions, Guid[] walletIdentifierIds, Dictionary<AssetType, decimal> balances) 
        where T : Models.Transactions.BaseTransaction
    {
        foreach (var tx in transactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            if (!tx.HaveBothWalletsSameAccountClassification() && tx.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
            
            var assetType = tx.IsReceiver(relevantWalletId) ?
                tx.ReceiverWalletIdentifier.AssetType :
                tx.SenderWalletIdentifier.AssetType;
            
            if (!balances.ContainsKey(assetType)) 
                balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }
    }
}