using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Validation;
using SFManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SFManagement.Infrastructure.Data;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.Services.Assets;

/// <summary>
/// Summary class for initial balance information
/// </summary>
public class InitialBalanceSummary
{
    public Guid BaseAssetHolderId { get; set; }
    public string BaseAssetHolderName { get; set; } = string.Empty;
    public List<InitialBalance> AssetTypeBalances { get; set; } = new();
    public List<InitialBalance> AssetGroupBalances { get; set; } = new();
    public int TotalAssetTypeBalances { get; set; }
    public int TotalAssetGroupBalances { get; set; }
    public DateTime? LastUpdated { get; set; }
}

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
        AssetType assetType, 
        decimal balance,
        AssetType? balanceAs = null,
        decimal? conversionRate = null)
    {
        // Validate BaseAssetHolder exists
        var baseAssetHolder = await context.BaseAssetHolders
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (baseAssetHolder == null)
            throw new ArgumentException($"BaseAssetHolder not found: {baseAssetHolderId}");

        // Validate conversion parameters
        if (balanceAs.HasValue && !conversionRate.HasValue)
            throw new ArgumentException("ConversionRate is required when BalanceAs is specified");

        if (!balanceAs.HasValue && conversionRate.HasValue)
            throw new ArgumentException("BalanceAs is required when ConversionRate is specified");

        if (conversionRate.HasValue && conversionRate <= 0)
            throw new ArgumentException("ConversionRate must be positive");

        // Check for existing active initial balance for this combination
        var existingBalance = await GetActiveInitialBalance(baseAssetHolderId, assetType);
        
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
            AssetType = assetType,
            AssetGroup = 0, // Must be 0 when AssetType is set
            Balance = balance,
            BalanceAs = balanceAs,
            ConversionRate = conversionRate,
        };

        return await Add(initialBalance);
    }

    /// <summary>
    /// Creates or updates an initial balance for a BaseAssetHolder and AssetGroup
    /// Only one active initial balance per BaseAssetHolder/AssetGroup combination is allowed
    /// </summary>
    public async Task<InitialBalance> SetInitialBalanceForAssetGroup(
        Guid baseAssetHolderId, 
        AssetGroup assetGroup, 
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
        if (balanceAs.HasValue && !conversionRate.HasValue)
            throw new ArgumentException("ConversionRate is required when BalanceAs is specified");

        if (!balanceAs.HasValue && conversionRate.HasValue)
            throw new ArgumentException("BalanceAs is required when ConversionRate is specified");

        if (conversionRate.HasValue && conversionRate <= 0)
            throw new ArgumentException("ConversionRate must be positive");

        // Check for existing active initial balance for this combination
        var existingBalance = await GetActiveInitialBalanceForAssetGroup(baseAssetHolderId, assetGroup);
        
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
            AssetGroup = assetGroup,
            AssetType = 0, // Must be 0 (None) when AssetGroup is set
            Balance = balance,
            BalanceAs = balanceAs,
            ConversionRate = conversionRate,
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
                                      ib.AssetType == assetType && 
                                      ib.AssetGroup == 0 &&
                                      !ib.DeletedAt.HasValue);
    }

    /// <summary>
    /// Gets the active initial balance for a BaseAssetHolder and AssetGroup
    /// </summary>
    public async Task<InitialBalance?> GetActiveInitialBalanceForAssetGroup(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        return await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .FirstOrDefaultAsync(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                                      ib.AssetGroup == assetGroup && 
                                      ib.AssetType == 0 &&
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
            .OrderBy(ib => ib.AssetType)
            .ThenBy(ib => ib.AssetGroup)
            .ToListAsync();
    }

    /// <summary>
    /// Gets initial balances grouped by AssetType for a BaseAssetHolder
    /// </summary>
    public async Task<Dictionary<AssetType, InitialBalance>> GetInitialBalancesByAssetType(Guid baseAssetHolderId)
    {
        var initialBalances = await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .Where(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                        ib.AssetGroup == 0 && 
                        !ib.DeletedAt.HasValue)
            .ToListAsync();
        
        return initialBalances
            .Where(ib => ib.IsActive)
            .ToDictionary(ib => ib.AssetType, ib => ib);
    }

    /// <summary>
    /// Gets initial balances grouped by AssetGroup for a BaseAssetHolder
    /// </summary>
    public async Task<Dictionary<AssetGroup, InitialBalance>> GetInitialBalancesByAssetGroup(Guid baseAssetHolderId)
    {
        var initialBalances = await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .Where(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                        ib.AssetType == 0 && 
                        !ib.DeletedAt.HasValue)
            .ToListAsync();
        
        return initialBalances
            .Where(ib => ib.IsActive)
            .ToDictionary(ib => ib.AssetGroup, ib => ib);
    }

    /// <summary>
    /// Removes an initial balance by AssetType (soft delete)
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
    /// Removes an initial balance by AssetGroup (soft delete)
    /// </summary>
    public async Task<bool> RemoveInitialBalanceForAssetGroup(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        var initialBalance = await GetActiveInitialBalanceForAssetGroup(baseAssetHolderId, assetGroup);
        
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
    public async Task<List<InitialBalance>> GetInitialBalanceHistoryByAssetType(Guid baseAssetHolderId, AssetType assetType)
    {
        return await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .Where(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                        ib.AssetType == assetType && 
                        ib.AssetGroup == 0)
            .OrderByDescending(ib => ib.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets initial balance history for a BaseAssetHolder and AssetGroup
    /// Includes deleted/inactive balances for audit purposes
    /// </summary>
    public async Task<List<InitialBalance>> GetInitialBalanceHistoryByAssetGroup(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        return await context.InitialBalances
            .Include(ib => ib.BaseAssetHolder)
            .Where(ib => ib.BaseAssetHolderId == baseAssetHolderId && 
                        ib.AssetGroup == assetGroup && 
                        ib.AssetType == 0)
            .OrderByDescending(ib => ib.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Validates initial balance data before creation/update
    /// </summary>
    public async Task<List<string>> ValidateInitialBalance(
        Guid baseAssetHolderId, 
        AssetType? assetType = null,
        AssetGroup? assetGroup = null,
        decimal balance = 0,
        AssetType? balanceAs = null,
        decimal? conversionRate = null)
    {
        var errors = new List<string>();

        // Validate BaseAssetHolder exists
        var baseAssetHolderExists = await context.BaseAssetHolders
            .AnyAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (!baseAssetHolderExists)
            errors.Add("BaseAssetHolder not found or is deleted");

        // Validate that either AssetType or AssetGroup is specified, but not both
        var hasAssetType = assetType.HasValue && assetType != 0;
        var hasAssetGroup = assetGroup.HasValue && assetGroup != 0;
        
        if (hasAssetType && hasAssetGroup)
            errors.Add("AssetGroup and AssetType cannot be set at the same time");

        if (!hasAssetType && !hasAssetGroup)
            errors.Add("Either AssetType or AssetGroup must be specified");

        // Balance amount can be negative (removed validation)
        // Negative balances are allowed for initial balance adjustments

        // Validate conversion parameters (can be used for both AssetType and AssetGroup)
        if (balanceAs.HasValue && conversionRate.HasValue && conversionRate <= 0)
            errors.Add("ConversionRate must be positive when specified");

        if (balanceAs.HasValue && !conversionRate.HasValue)
            errors.Add("ConversionRate is required when BalanceAs is specified");

        if (!balanceAs.HasValue && conversionRate.HasValue)
            errors.Add("BalanceAs is required when ConversionRate is specified");

        return errors;
    }

    /// <summary>
    /// Creates or updates an initial balance for a BaseAssetHolder
    /// Handles both AssetType and AssetGroup scenarios in a single method
    /// </summary>
    public async Task<InitialBalance> SetInitialBalanceUnified(
        Guid baseAssetHolderId,
        AssetType? assetType = null,
        AssetGroup? assetGroup = null,
        decimal balance = 0,
        AssetType? balanceAs = null,
        decimal? conversionRate = null,
        string? description = null)
    {
        // Validate the input using the existing validation method
        var validationErrors = await ValidateInitialBalance(
            baseAssetHolderId, assetType, assetGroup, balance, balanceAs, conversionRate);
        
        if (validationErrors.Any())
        {
            throw new ArgumentException($"Validation failed: {string.Join(", ", validationErrors)}");
        }

        // Validate BaseAssetHolder exists
        var baseAssetHolder = await context.BaseAssetHolders
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (baseAssetHolder == null)
            throw new ArgumentException($"BaseAssetHolder not found: {baseAssetHolderId}");

        InitialBalance? existingBalance = null;
        
        // Check for existing balance based on whether it's AssetType or AssetGroup
        if (assetType.HasValue && assetType != 0)
        {
            existingBalance = await GetActiveInitialBalance(baseAssetHolderId, assetType.Value);
        }
        else if (assetGroup.HasValue && assetGroup != 0)
        {
            existingBalance = await GetActiveInitialBalanceForAssetGroup(baseAssetHolderId, assetGroup.Value);
        }
        
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
            AssetType = assetType ?? 0,
            AssetGroup = assetGroup ?? 0,
            Balance = balance,
            BalanceAs = balanceAs,
            ConversionRate = conversionRate,
        };

        return await Add(initialBalance);
    }

    /// <summary>
    /// Gets the effective balance for a specific AssetType for a BaseAssetHolder
    /// Returns null if no initial balance is set
    /// </summary>
    public async Task<decimal?> GetEffectiveBalanceForAssetType(Guid baseAssetHolderId, AssetType assetType)
    {
        var initialBalance = await GetActiveInitialBalance(baseAssetHolderId, assetType);
        return initialBalance?.EffectiveBalance;
    }

    /// <summary>
    /// Gets the effective balance for a specific AssetGroup for a BaseAssetHolder
    /// Returns null if no initial balance is set
    /// </summary>
    public async Task<decimal?> GetEffectiveBalanceForAssetGroup(Guid baseAssetHolderId, AssetGroup assetGroup)
    {
        var initialBalance = await GetActiveInitialBalanceForAssetGroup(baseAssetHolderId, assetGroup);
        return initialBalance?.EffectiveBalance;
    }

    /// <summary>
    /// Gets a summary of all initial balances for a BaseAssetHolder
    /// </summary>
    public async Task<InitialBalanceSummary> GetInitialBalanceSummary(Guid baseAssetHolderId)
    {
        var baseAssetHolder = await context.BaseAssetHolders
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);

        if (baseAssetHolder == null)
            throw new ArgumentException($"BaseAssetHolder not found: {baseAssetHolderId}");

        var allBalances = await GetInitialBalancesForAssetHolder(baseAssetHolderId);
        
        var assetTypeBalances = allBalances.Where(ib => ib.AssetType != 0).ToList();
        var assetGroupBalances = allBalances.Where(ib => ib.AssetGroup != 0).ToList();

        return new InitialBalanceSummary
        {
            BaseAssetHolderId = baseAssetHolderId,
            BaseAssetHolderName = baseAssetHolder.Name,
            AssetTypeBalances = assetTypeBalances,
            AssetGroupBalances = assetGroupBalances,
            TotalAssetTypeBalances = assetTypeBalances.Count,
            TotalAssetGroupBalances = assetGroupBalances.Count,
            LastUpdated = allBalances.Any() ? allBalances.Max(ib => ib.UpdatedAt ?? ib.CreatedAt) : null
        };
    }

    /// <summary>
    /// Checks if a BaseAssetHolder has any initial balances set
    /// </summary>
    public async Task<bool> HasInitialBalances(Guid baseAssetHolderId)
    {
        return await context.InitialBalances
            .AnyAsync(ib => ib.BaseAssetHolderId == baseAssetHolderId && !ib.DeletedAt.HasValue);
    }

    /// <summary>
    /// Gets the total count of initial balances for a BaseAssetHolder
    /// </summary>
    public async Task<int> GetInitialBalanceCount(Guid baseAssetHolderId)
    {
        return await context.InitialBalances
            .CountAsync(ib => ib.BaseAssetHolderId == baseAssetHolderId && !ib.DeletedAt.HasValue);
    }
}