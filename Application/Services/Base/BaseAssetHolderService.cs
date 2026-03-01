using SFManagement.Application.DTOs.Statements;
using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Transactions;
using SFManagement.Application.Services.Support;
using SFManagement.Application.Services.Assets;
using SFManagement.Infrastructure.Data;
using SFManagement.Infrastructure.Logging;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Exceptions;
using SFManagement.Domain.Interfaces;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Entities.Support;
using Microsoft.EntityFrameworkCore;
using SFManagement.Domain.Common;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Enums.Assets;
using SFManagement.Domain.Enums.Metadata;

namespace SFManagement.Application.Services.Base;

// implement .AsNoTracking() to make the query faster
// context.Banks.AsNoTracking().FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);

public class BaseAssetHolderService<TEntity>(DataContext context, IHttpContextAccessor httpContextAccessor, IAssetHolderDomainService domainService, ReferralService referralService, InitialBalanceService initialBalanceService) 
    : BaseService<TEntity>(context, httpContextAccessor) where TEntity : BaseDomain, IAssetHolder
{
    protected readonly IAssetHolderDomainService _domainService = domainService;
    protected readonly ReferralService _referralService = referralService;
    protected readonly InitialBalanceService _initialBalanceService = initialBalanceService;

    // Strategy pattern dictionaries to replace typeof() checks
    private static readonly Dictionary<Type, Func<IQueryable<TEntity>, IQueryable<TEntity>>> IncludeStrategies = new()
    {
        [typeof(Bank)] = query => ((IQueryable<Bank>)query).Include(b => b.BaseAssetHolder).Cast<TEntity>(),
        [typeof(Client)] = query => ((IQueryable<Client>)query).Include(c => c.BaseAssetHolder).Cast<TEntity>(),
        [typeof(Member)] = query => ((IQueryable<Member>)query).Include(m => m.BaseAssetHolder).Cast<TEntity>(),
        [typeof(PokerManager)] = query => ((IQueryable<PokerManager>)query).Include(pm => pm.BaseAssetHolder).Cast<TEntity>()
    };

    private static readonly Dictionary<Type, Func<DataContext, Guid, Task<TEntity?>>> EntityGetStrategies = new()
    {
        [typeof(Bank)] = async (ctx, id) => 
        {
            var bank = await ctx.Banks.FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            return bank as TEntity;
        },
        [typeof(Client)] = async (ctx, id) => 
        {
            var client = await ctx.Clients.FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            return client as TEntity;
        },
        [typeof(Member)] = async (ctx, id) => 
        {
            var member = await ctx.Members.FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            return member as TEntity;
        },
        [typeof(PokerManager)] = async (ctx, id) => 
        {
            var pokerManager = await ctx.PokerManagers.FirstOrDefaultAsync(x => x.BaseAssetHolderId == id && !x.DeletedAt.HasValue);
            return pokerManager as TEntity;
        }
    };

    private static readonly Dictionary<Type, Func<IQueryable<BaseAssetHolder>, IQueryable<BaseAssetHolder>>> FilterStrategies = new()
    {
        [typeof(Client)] = query => query.Include(bah => bah.Client).Where(bah => bah.Client != null),
        [typeof(Bank)] = query => query.Include(bah => bah.Bank).Where(bah => bah.Bank != null),
        [typeof(Member)] = query => query.Include(bah => bah.Member).Where(bah => bah.Member != null),
        [typeof(PokerManager)] = query => query.Include(bah => bah.PokerManager).Where(bah => bah.PokerManager != null)
    };

    private static readonly Dictionary<Type, AssetHolderType> EntityTypeMapping = new()
    {
        [typeof(Client)] = AssetHolderType.Client,
        [typeof(Bank)] = AssetHolderType.Bank,
        [typeof(Member)] = AssetHolderType.Member,
        [typeof(PokerManager)] = AssetHolderType.PokerManager
    };

    public override async Task<List<TEntity>> List()
    {
        var query = _entity.AsQueryable();
        
        // Apply include strategy based on entity type
        if (IncludeStrategies.TryGetValue(typeof(TEntity), out var includeStrategy))
        {
            query = includeStrategy(query);
        }
        
        return await query.AsNoTracking().Where(x => !x.BaseAssetHolder.DeletedAt.HasValue)
        .OrderByDescending(x => x.BaseAssetHolder.CreatedAt)
        .ToListAsync();
    }

    public override async Task<TEntity?> Get(Guid id)
    {
        // Get the BaseAssetHolder first since the ID is always from BaseAssetHolder
        var baseAssetHolder = await context.BaseAssetHolders
            .Include(c => c.AssetPools)
                .ThenInclude(aw => aw.WalletIdentifiers)
            .Include(c => c.Addresses)
            .Include(c => c.ContactPhones)
            .Include(c => c.InitialBalances)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
        
        if (baseAssetHolder == null)
            return null;
            
        // Handle BaseAssetHolder directly
        if (typeof(TEntity) == typeof(BaseAssetHolder))
        {
            return baseAssetHolder as TEntity;
        }
            
        // Use strategy pattern to get specific entity
        if (EntityGetStrategies.TryGetValue(typeof(TEntity), out var getStrategy))
        {
            var entity = await getStrategy(context, id);
            if (entity != null)
            {
                // Set BaseAssetHolder property using reflection for type safety
                var baseAssetHolderProperty = typeof(TEntity).GetProperty("BaseAssetHolder");
                baseAssetHolderProperty?.SetValue(entity, baseAssetHolder);
                return entity;
            }
        }
        
        // Fallback to base implementation for other entity types
        return await base.Get(id);
    }

    /// <summary>
    /// Generic method to create an asset holder entity with validation
    /// </summary>
    /// <typeparam name="TRequest">The request type that inherits from BaseAssetHolderRequest</typeparam>
    /// <param name="request">The request object</param>
    /// <param name="entityFactory">Factory function to create the specific entity</param>
    /// <param name="validationMethod">Domain service validation method</param>
    /// <returns>The created entity with BaseAssetHolder included</returns>
    public async Task<TEntity> AddFromRequest<TRequest>(
        TRequest request, 
        Func<BaseAssetHolder, TEntity> entityFactory,
        Func<TRequest, Task<DomainValidationResult>> validationMethod) 
        where TRequest : BaseAssetHolderRequest
    {
        // Validate using domain service
        var validationResult = await validationMethod(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        try
        {
            // Create BaseAssetHolder using helper method
            var baseAssetHolder = await CreateBaseAssetHolder(
                request.Name,
                request.GovernmentNumber,
                request.TaxEntityType,
                request.ReferrerId
            );

            // Create specific entity using the factory
            var entity = entityFactory(baseAssetHolder);

            // Use base service to add entity (handles audit automatically)
            var result = await base.Add(entity);

            // Return the entity with BaseAssetHolder included
            var createdEntity = await Get(result.BaseAssetHolderId);

            if (createdEntity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity).Name, result.Id);
            }

            return createdEntity;
        }
        catch (ValidationException)
        {
            throw; // Re-throw validation exceptions
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to create {typeof(TEntity).Name.ToLower()}", ex, $"{typeof(TEntity).Name.ToUpper()}_CREATION_FAILED");
        }
    }

    /// <summary>
    /// Generic method to update an asset holder entity with validation
    /// </summary>
    /// <typeparam name="TRequest">The request type that inherits from BaseAssetHolderRequest</typeparam>
    /// <param name="entityId">The entity ID</param>
    /// <param name="request">The request object</param>
    /// <param name="updateAction">Action to update entity-specific properties</param>
    /// <param name="validationMethod">Domain service validation method</param>
    /// <returns>The updated entity</returns>
    public async Task<TEntity> UpdateFromRequest<TRequest>(
        Guid entityId, 
        TRequest request,
        Action<TEntity, TRequest> updateAction,
        Func<TRequest, Task<DomainValidationResult>> validationMethod) 
        where TRequest : BaseAssetHolderRequest
    {
        try
        {
            var existingEntity = await Get(entityId);
            if (existingEntity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity).Name, entityId);
            }

            // Set the BaseAssetHolderId for validation
            request.BaseAssetHolderId = existingEntity.BaseAssetHolderId;

            // Validate using domain service
            var validationResult = await validationMethod(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            // Update BaseAssetHolder
            var baseAssetHolder = await context.BaseAssetHolders.FindAsync(existingEntity.BaseAssetHolderId);
            if (baseAssetHolder != null)
            {
                baseAssetHolder.Name = request.Name;
                baseAssetHolder.GovernmentNumber = request.GovernmentNumber;
                baseAssetHolder.TaxEntityType = request.TaxEntityType;
                baseAssetHolder.UpdatedAt = DateTime.UtcNow;
            }

            // Update entity-specific properties
            updateAction(existingEntity, request);
            existingEntity.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return existingEntity;
        }
        catch (ValidationException)
        {
            throw;
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to update {typeof(TEntity).Name.ToLower()}", ex, $"{typeof(TEntity).Name.ToUpper()}_UPDATE_FAILED");
        }
    }

    /// <summary>
    /// Validates if an asset holder can be deleted
    /// </summary>
    public async Task<bool> CanDelete(Guid entityId)
    {
        try
        {
            var entity = await Get(entityId);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity).Name, entityId);
            }

            return await _domainService.CanDeleteAssetHolder(entity.BaseAssetHolderId);
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to check if {typeof(TEntity).Name.ToLower()} can be deleted", ex, $"{typeof(TEntity).Name.ToUpper()}_DELETE_CHECK_FAILED");
        }
    }

    /// <summary>
    /// Soft deletes an asset holder with business rule validation
    /// </summary>
    public async Task<bool> DeleteWithValidation(Guid entityId)
    {
        try
        {
            var canDelete = await CanDelete(entityId);
            if (!canDelete)
            {
                throw new BusinessRuleException($"{typeof(TEntity).Name.ToUpper()}_HAS_DEPENDENCIES", 
                    $"Cannot delete {typeof(TEntity).Name.ToLower()} because it has active transactions or dependencies");
            }

            var assetHolder = await Get(entityId);
            if (assetHolder == null || assetHolder.BaseAssetHolder == null)
            {
                throw new EntityNotFoundException(typeof(TEntity).Name, entityId);
            }

            // Also soft delete the AssetHolder (bank, client, member, pokerManager)
            await Delete(assetHolder.Id);

            var baseAssetHolder = await context.BaseAssetHolders.FindAsync(assetHolder.BaseAssetHolderId) ?? throw new EntityNotFoundException(typeof(BaseAssetHolder).Name, assetHolder.BaseAssetHolderId);
            
            baseAssetHolder.DeletedAt = DateTime.UtcNow;

            // Soft delete the entity (baseAssetHolder)
            await Delete(baseAssetHolder.Id);

            await context.SaveChangesAsync();

            return true;
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to delete {typeof(TEntity).Name.ToLower()}", ex, $"{typeof(TEntity).Name.ToUpper()}_DELETE_FAILED");
        }
    }

    /// <summary>
    /// Gets asset holder statistics
    /// </summary>
    public async Task<AssetHolderStatistics> GetAssetHolderStatistics(Guid entityId)
    {
        try
        {
            var entity = await Get(entityId);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(TEntity).Name, entityId);
            }

            var hasActiveTransactions = await _domainService.HasActiveTransactions(entity.BaseAssetHolderId);
            var totalBalance = await _domainService.GetTotalBalance(entity.BaseAssetHolderId);
            var hasActiveAssetPools = await _domainService.HasActiveAssetPools(entity.BaseAssetHolderId);

            return new AssetHolderStatistics
            {
                EntityId = entityId,
                BaseAssetHolderId = entity.BaseAssetHolderId,
                HasActiveTransactions = hasActiveTransactions,
                TotalBalance = totalBalance,
                HasActiveAssetPools = hasActiveAssetPools,
                CanBeDeleted = await _domainService.CanDeleteAssetHolder(entity.BaseAssetHolderId),
                AssetHolderType = GetAssetHolderTypeForEntity()
            };
        }
        catch (EntityNotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to get {typeof(TEntity).Name.ToLower()} statistics", ex, $"{typeof(TEntity).Name.ToUpper()}_STATISTICS_FAILED");
        }
    }

    // this method may be removed
    public async Task<Guid[]> GetAssetHolderAssetPoolIds()
    {
        var assetHolderType = GetAssetHolderTypeForEntity();
        
        // Use navigation properties instead of computed AssetHolderType property
        IQueryable<BaseAssetHolder> query = assetHolderType switch
        {
            AssetHolderType.Client => context.BaseAssetHolders.Where(bah => bah.Client != null),
            AssetHolderType.Bank => context.BaseAssetHolders.Where(bah => bah.Bank != null),
            AssetHolderType.Member => context.BaseAssetHolders.Where(bah => bah.Member != null),
            AssetHolderType.PokerManager => context.BaseAssetHolders.Where(bah => bah.PokerManager != null),
            _ => throw new InvalidOperationException($"Unknown asset holder type: {assetHolderType}")
        };
        
        var assetPoolIds = await query
            .Include(bah => bah.AssetPools)
            .SelectMany(bah => bah.AssetPools.Where(aw => !aw.DeletedAt.HasValue).Select(aw => aw.Id))
            .ToArrayAsync();
        
        return assetPoolIds;
    }
    
    public async Task<List<BaseAssetHolder>> GetFilteredByWalletIdentifierType(AssetType assetType)
    {
        var baseAssetHolders = await context.BaseAssetHolders
            .Include(bah => bah.AssetPools)
            .ThenInclude(aw => aw.WalletIdentifiers.Where(wi => !wi.DeletedAt.HasValue))
            .Where(bah => !bah.DeletedAt.HasValue)
            .ToListAsync();

        return baseAssetHolders
            .Where(bah => bah.AssetPools.Any(aw => 
                aw.WalletIdentifiers.Any(wi => wi.AssetType == assetType && !wi.DeletedAt.HasValue)))
            .ToList();
    }

    // Helper method to get AssetHolderType for the current entity type
    private AssetHolderType GetAssetHolderTypeForEntity()
    {
        if (EntityTypeMapping.TryGetValue(typeof(TEntity), out var assetHolderType))
        {
            return assetHolderType;
        }
        
        throw new InvalidOperationException($"Unknown entity type: {typeof(TEntity).Name}");
    }

    // add variable BaseAssetWalletType so the balance could have different behavior
    public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(
        Guid baseAssetHolderId,
        DateTime? asOfDate = null)
    {
        var balances = new Dictionary<AssetType, decimal>();

        var assetHolder = await context.BaseAssetHolders
        .Include(bah => bah.InitialBalances)
        .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);

        if (assetHolder != null)
        {
            foreach (var initialBalance in assetHolder.InitialBalances)
            {
                if (initialBalance.AssetType != AssetType.None && initialBalance.AssetGroup == AssetGroup.None)
                balances[initialBalance.AssetType] = initialBalance.Balance;
            }
        }

        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => wi.AssetPool!.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
        
        var pokerManagerIds = await context.PokerManagers
            .Select(pm => pm.BaseAssetHolderId)
            .ToListAsync();
        var pokerManagerIdSet = new HashSet<Guid>(pokerManagerIds);

        var rakeOverrideManagerIdsForType = await context.PokerManagers
            .Where(pm => pm.ManagerProfitType == ManagerProfitType.RakeOverrideCommission && !pm.DeletedAt.HasValue)
            .Select(pm => pm.BaseAssetHolderId)
            .ToListAsync();
        var rakeOverrideManagerIdSetForType = new HashSet<Guid>(rakeOverrideManagerIdsForType);
       
        var digitalQuery = context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue &&
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) ||
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)));

        if (asOfDate.HasValue)
        {
            digitalQuery = digitalQuery.Where(dt => dt.Date <= asOfDate.Value);
        }

        var digitalTransactions = await digitalQuery
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .ToArrayAsync() ?? [];
            

        var fiatQuery = context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue &&
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) ||
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)));

        if (asOfDate.HasValue)
        {
            fiatQuery = fiatQuery.Where(ft => ft.Date <= asOfDate.Value);
        }

        var fiatTransactions = await fiatQuery
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .ToArrayAsync() ?? [];

        var settlementQuery = context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue &&
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) ||
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)));

        if (asOfDate.HasValue)
        {
            settlementQuery = settlementQuery.Where(st => st.Date <= asOfDate.Value);
        }

        var settlementTransactions = await settlementQuery
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .ToArrayAsync() ?? [];

        // Process FiatAssetTransactions
        foreach (var tx in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            // signal inversion for the liability wallet when the account classification is different
            // asset accounts are aligned with the transaction signal while liability accounts have the opposite signal
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            if (!tx.HaveBothWalletsSameAccountClassification() && tx.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
            
            var assetType = tx.IsReceiver(relevantWalletId) ?
                tx.ReceiverWalletIdentifier!.AssetType :
                tx.SenderWalletIdentifier!.AssetType;
            
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        // Process DigitalAssetTransactions
        foreach (var tx in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            // signal inversion when the wallets have  different account classification
            // asset accounts are aligned with the transaction signal while liability accounts have the opposite signal
            if (!tx.HaveBothWalletsSameAccountClassification() && tx.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
            
            if (tx.BalanceAs != null && tx.ConversionRate != null)
            {
                if (!balances.ContainsKey(tx.BalanceAs.Value)) balances[tx.BalanceAs.Value] = 0;
                balances[tx.BalanceAs.Value] += signedAmount * tx.ConversionRate.Value;
                    continue;
                }

            if (tx.Rate.HasValue && tx.Rate.Value > 0)
            {
                signedAmount = signedAmount * 100m / (100m + tx.Rate.Value);
            }
                
            var assetType = tx.IsReceiver(relevantWalletId) ?
                tx.ReceiverWalletIdentifier!.AssetType :
                tx.SenderWalletIdentifier!.AssetType;

            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        // Process SettlementTransactions
        foreach (var tx in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var relevantWallet = walletIdentifiers.First(wi => wi.Id == relevantWalletId);
            var settlementAssetHolderId = relevantWallet.AssetPool?.BaseAssetHolderId;
            if (!settlementAssetHolderId.HasValue)
            {
                continue;
            }
            var isPokerManager = pokerManagerIdSet.Contains(settlementAssetHolderId.Value);

            // Rake impact
            var rakeImpact = isPokerManager
                ? -(tx.RakeAmount * (tx.RakeCommission / 100m))
                : tx.RakeAmount * ((tx.RakeBack ?? 0m) / 100m);

            if (!balances.ContainsKey(AssetType.BrazilianReal)) balances[AssetType.BrazilianReal] = 0;
            balances[AssetType.BrazilianReal] += rakeImpact;

            // For RakeOverrideCommission managers, reflect AssetAmount in both chip and BRL balances.
            // The chip type is determined by the wallet's AssetType (e.g., PokerStars).
            // Both use the same signal: receiver +, sender -
            if (settlementAssetHolderId.HasValue && rakeOverrideManagerIdSetForType.Contains(settlementAssetHolderId.Value))
            {
                var isReceiver = tx.ReceiverWalletIdentifierId == relevantWalletId;
                var signedChipAmount = isReceiver ? tx.AssetAmount : -tx.AssetAmount;

                var chipAssetType = isReceiver
                    ? tx.ReceiverWalletIdentifier?.AssetType ?? AssetType.None
                    : tx.SenderWalletIdentifier?.AssetType ?? AssetType.None;

                if (chipAssetType != AssetType.None)
                {
                    if (!balances.ContainsKey(chipAssetType)) balances[chipAssetType] = 0;
                    balances[chipAssetType] += signedChipAmount;
                }

                balances[AssetType.BrazilianReal] += signedChipAmount;
            }
        }

        return balances;
    }
    
    // add variable BaseAssetWalletType so the balance could have different behavior
    // calculates balance for PokerManager
    public async Task<Dictionary<AssetGroup, decimal>> GetBalancesByAssetGroup(
        Guid baseAssetHolderId,
        DateTime? asOfDate = null)
    {
        var balances = new Dictionary<AssetGroup, decimal>();

        var assetHolder = await context.BaseAssetHolders
            .Include(bah => bah.InitialBalances)
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);

        if (assetHolder != null)
        {
            foreach (var initialBalance in assetHolder.InitialBalances)
            {
                if (initialBalance.AssetGroup != AssetGroup.None && initialBalance.AssetType == AssetType.None)
                    balances[initialBalance.AssetGroup] = initialBalance.Balance;
            }
        }

        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => wi.AssetPool!.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
        var pokerManagerIds = await context.PokerManagers
            .Select(pm => pm.BaseAssetHolderId)
            .ToListAsync();
        var pokerManagerIdSet = new HashSet<Guid>(pokerManagerIds);

        var rakeOverrideManagerIds = await context.PokerManagers
            .Where(pm => pm.ManagerProfitType == ManagerProfitType.RakeOverrideCommission && !pm.DeletedAt.HasValue)
            .Select(pm => pm.BaseAssetHolderId)
            .ToListAsync();
        var rakeOverrideManagerIdSet = new HashSet<Guid>(rakeOverrideManagerIds);
        
        if (!walletIdentifierIds.Any())
            return balances;

        // Get all transactions for these wallet identifiers
        var fiatQuery = context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue &&
                        (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)));

        if (asOfDate.HasValue)
        {
            fiatQuery = fiatQuery.Where(ft => ft.Date <= asOfDate.Value);
        }

        var fiatTransactions = await fiatQuery
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
            .ToListAsync();

        var digitalQuery = context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue &&
                        (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)));

        if (asOfDate.HasValue)
        {
            digitalQuery = digitalQuery.Where(dt => dt.Date <= asOfDate.Value);
        }

        var digitalTransactions = await digitalQuery
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
            .ToListAsync();

        var settlementQuery = context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue &&
                        (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) ||
                         walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)));

        if (asOfDate.HasValue)
        {
            settlementQuery = settlementQuery.Where(st => st.Date <= asOfDate.Value);
        }

        var settlementTransactions = await settlementQuery
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
            .ToListAsync();

        // Process FiatAssetTransactions
        foreach (var tx in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            if (!tx.HaveBothWalletsSameAccountClassification() && tx.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
            
            var assetGroup = tx.IsReceiver(relevantWalletId) ?
                tx.ReceiverWalletIdentifier!.AssetGroup :
                tx.SenderWalletIdentifier!.AssetGroup;
            
            if (assetGroup == AssetGroup.Flexible)
            {
                assetGroup = tx.ReceiverWalletIdentifier!.AssetType == AssetType.BrazilianReal ? AssetGroup.FiatAssets : AssetGroup.PokerAssets;
            }
            
            if (!balances.ContainsKey(assetGroup)) balances[assetGroup] = 0;
            balances[assetGroup] += signedAmount;
        }

        // Process DigitalAssetTransactions
        foreach (var tx in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            // signal inversion for the liability wallet when the account classification is different
            // asset accounts are aligned with the transaction signal while liability accounts have the opposite signal
            if (!tx.HaveBothWalletsSameAccountClassification() && tx.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
            
            var senderAssetGroup = tx.SenderWalletIdentifier?.AssetPool?.AssetGroup ?? AssetGroup.None;
            var receiverAssetGroup = tx.ReceiverWalletIdentifier?.AssetPool?.AssetGroup ?? AssetGroup.None;

            var isSelfConversion =
                walletIdentifierIds.Contains(tx.SenderWalletIdentifierId) &&
                walletIdentifierIds.Contains(tx.ReceiverWalletIdentifierId) &&
                ((senderAssetGroup == AssetGroup.Flexible && receiverAssetGroup == AssetGroup.PokerAssets) ||
                 (senderAssetGroup == AssetGroup.PokerAssets && receiverAssetGroup == AssetGroup.Flexible)) &&
                tx.BalanceAs != null &&
                tx.ConversionRate != null;

            if (isSelfConversion)
            {
                var isEntering = senderAssetGroup == AssetGroup.Flexible;

                var pokerAssetsImpact = isEntering ? tx.AssetAmount : -tx.AssetAmount;
                pokerAssetsImpact = pokerAssetsImpact * (100 - (tx.Rate ?? 0)) / 100;

            var conversionRate = tx.ConversionRate.GetValueOrDefault();
            var fiatAssetsImpact = isEntering
                ? tx.AssetAmount * conversionRate
                : -tx.AssetAmount * conversionRate;

                if (!balances.ContainsKey(AssetGroup.PokerAssets)) balances[AssetGroup.PokerAssets] = 0;
                balances[AssetGroup.PokerAssets] += pokerAssetsImpact;

                if (!balances.ContainsKey(AssetGroup.FiatAssets)) balances[AssetGroup.FiatAssets] = 0;
                balances[AssetGroup.FiatAssets] += fiatAssetsImpact;

                continue;
            }
                
            var assetGroup = tx.IsReceiver(relevantWalletId) ?
                tx.ReceiverWalletIdentifier!.AssetGroup :
                tx.SenderWalletIdentifier!.AssetGroup;

            if (!balances.ContainsKey(assetGroup)) balances[assetGroup] = 0;
            balances[assetGroup] += signedAmount;
        }

        // Process SettlementTransactions
        foreach (var tx in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var relevantWallet = walletIdentifiers.First(wi => wi.Id == relevantWalletId);
            var settlementAssetHolderId = relevantWallet.AssetPool?.BaseAssetHolderId;
            if (!settlementAssetHolderId.HasValue)
            {
                continue;
            }
            var isPokerManager = pokerManagerIdSet.Contains(settlementAssetHolderId.Value);

            // Rake impact (existing logic): deducts rake cost for managers, adds rakeback for non-managers
            var rakeImpact = isPokerManager
                ? -(tx.RakeAmount * (tx.RakeCommission / 100m))
                : tx.RakeAmount * ((tx.RakeBack ?? 0m) / 100m);

            if (!balances.ContainsKey(AssetGroup.FiatAssets)) balances[AssetGroup.FiatAssets] = 0;
            balances[AssetGroup.FiatAssets] += rakeImpact;

            // For RakeOverrideCommission managers, the settlement's AssetAmount represents
            // chips transferred that must also be reflected in both PokerAssets and FiatAssets.
            // Both use the same signal: receiver +, sender -
            if (settlementAssetHolderId.HasValue && rakeOverrideManagerIdSet.Contains(settlementAssetHolderId.Value))
            {
                var isReceiver = tx.ReceiverWalletIdentifierId == relevantWalletId;
                var signedChipAmount = isReceiver ? tx.AssetAmount : -tx.AssetAmount;

                if (!balances.ContainsKey(AssetGroup.PokerAssets)) balances[AssetGroup.PokerAssets] = 0;
                balances[AssetGroup.PokerAssets] += signedChipAmount;

                balances[AssetGroup.FiatAssets] += signedChipAmount;
            }
        }

        return balances;
    }
    
    public async Task<StatementAssetHolderWithTransactions> GetTransactionsStatementForAssetHolder(Guid baseAssetHolderId)
    {
        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .Where(wi => wi.AssetPool!.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
                .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
    
        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.Category)
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];

        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.Category)
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.Category)
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];
        
        var allTransactions = new List<StatementTransactionResponse>();
        
        // Process digital asset transactions using helper methods
        foreach (var dat in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                dat.SenderWalletIdentifierId == id || dat.ReceiverWalletIdentifierId == id);
            var relevantWallet = walletIdentifiers.FirstOrDefault(wi => wi.Id == relevantWalletId);
            var counterPartyWallet = dat.GetCounterpartyForWalletIdentifier(relevantWalletId);

            var signedAmount = dat.GetSignedAmountForWalletIdentifier(relevantWalletId);
            if (!dat.HaveBothWalletsSameAccountClassification() && dat.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = dat.Id,
                        Date = dat.Date,
                        Description = dat.Category?.Description,
                        CategoryName = dat.Category?.Description,
                AssetAmount = signedAmount,
                        BalanceAs = dat.BalanceAs,
                        ConversionRate = dat.ConversionRate,
                        Rate = dat.Rate,
                RateFeeAmount = (dat.BalanceAs == null && dat.Rate.HasValue && dat.Rate.Value > 0)
                    ? Math.Abs(signedAmount) * dat.Rate.Value / (100m + dat.Rate.Value)
                    : null,
                AssetType = dat.SenderWalletIdentifier!.AssetType,
                CounterPartyName = dat.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = dat.GetWalletIdentifierInput(relevantWalletId),
                CounterPartyWalletIdentifierInput = counterPartyWallet.GetPokerMetadata(PokerWalletMetadata.InputForTransactions),
                CounterPartyAssetHolderName = counterPartyWallet.AssetPool?.BaseAssetHolder?.Name,
                AssetGroup = relevantWallet?.AssetPool?.AssetGroup ?? dat.SenderWalletIdentifier!.AssetGroup
            });
        }

        // Process fiat asset transactions using helper methods
        foreach (var fat in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                fat.SenderWalletIdentifierId == id || fat.ReceiverWalletIdentifierId == id);
            var relevantWallet = walletIdentifiers.FirstOrDefault(wi => wi.Id == relevantWalletId);
            var counterPartyWallet = fat.GetCounterpartyForWalletIdentifier(relevantWalletId);

            var signedAmount = fat.GetSignedAmountForWalletIdentifier(relevantWalletId);
            if (!fat.HaveBothWalletsSameAccountClassification() && fat.IsWalletIdentifierLiability(relevantWalletId))
            {
                signedAmount = -signedAmount;
            }
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = fat.Id,
                        Date = fat.Date,
                        Description = fat.Category?.Description,
                        CategoryName = fat.Category?.Description,
                AssetAmount = signedAmount,
                        BalanceAs = null, // Fiat transactions don't have BalanceAs
                        ConversionRate = null, // Fiat transactions don't have ConversionRate
                        Rate = null, // Fiat transactions don't have Rate
                RateFeeAmount = null,
                AssetType = fat.SenderWalletIdentifier!.AssetType,
                CounterPartyName = fat.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = fat.GetWalletIdentifierInput(relevantWalletId),
                CounterPartyWalletIdentifierInput = counterPartyWallet.GetPokerMetadata(PokerWalletMetadata.InputForTransactions),
                CounterPartyAssetHolderName = counterPartyWallet.AssetPool?.BaseAssetHolder?.Name,
                AssetGroup = relevantWallet?.AssetPool?.AssetGroup ?? fat.SenderWalletIdentifier!.AssetGroup
            });
        }

        // Process settlement transactions using helper methods
        foreach (var st in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                st.SenderWalletIdentifierId == id || st.ReceiverWalletIdentifierId == id);
            var relevantWallet = walletIdentifiers.FirstOrDefault(wi => wi.Id == relevantWalletId);
            var counterPartyWallet = st.GetCounterpartyForWalletIdentifier(relevantWalletId);
                    
                    allTransactions.Add(new StatementTransactionResponse
            {
                Id = st.Id,
                Date = st.Date,
                Description = st.Category?.Description,
                CategoryName = st.Category?.Description,
                AssetAmount = st.GetSignedAmountForWalletIdentifier(relevantWalletId),
                BalanceAs = null, // Settlement transactions don't have BalanceAs
                ConversionRate = null, // Settlement transactions don't have ConversionRate
                Rate = null, // Settlement transactions don't have Rate
                RateFeeAmount = null,
                AssetType = st.SenderWalletIdentifier!.AssetType,
                CounterPartyName = st.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = st.GetWalletIdentifierInput(relevantWalletId),
                CounterPartyWalletIdentifierInput = counterPartyWallet.GetPokerMetadata(PokerWalletMetadata.InputForTransactions),
                CounterPartyAssetHolderName = counterPartyWallet.AssetPool?.BaseAssetHolder?.Name,
                AssetGroup = relevantWallet?.AssetPool?.AssetGroup ?? st.SenderWalletIdentifier!.AssetGroup,
                RakeAmount = st.RakeAmount,
                RakeCommission = st.RakeCommission,
                RakeBack = st.RakeBack,
                RakeBackAmount = st.RakeAmount * ((st.RakeBack ?? 0m) / 100m)
            });
        }
        
        return new StatementAssetHolderWithTransactions
        {
            Id = baseAssetHolderId,
            Name = context.BaseAssetHolders.FirstOrDefault(x => x.Id == baseAssetHolderId)?.Name ?? "",
            Transactions = allTransactions.OrderByDescending(t => t.Date).ToArray()
        };
    }

    // Helper method to create BaseAssetHolder using base service pattern
    protected async Task<BaseAssetHolder> CreateBaseAssetHolder(string name, string? governmentNumber = "", 
    TaxEntityType taxEntityType = TaxEntityType.CNPJ_Not_Taxable, Guid? referrerId = null)
    {
        var baseAssetHolder = new BaseAssetHolder
        {
            Name = name,
            GovernmentNumber = governmentNumber,
            TaxEntityType = taxEntityType,
            ReferrerId = referrerId
        };

        // Use the base service pattern for consistency
        await context.BaseAssetHolders.AddAsync(baseAssetHolder);
        await context.SaveChangesAsync();
        
        return baseAssetHolder;
    }
}

/// <summary>
/// Generic asset holder statistics data transfer object
/// </summary>
public class AssetHolderStatistics
{
    public Guid EntityId { get; set; }
    public Guid BaseAssetHolderId { get; set; }
    public bool HasActiveTransactions { get; set; }
    public decimal TotalBalance { get; set; }
    public bool HasActiveAssetPools { get; set; }
    public bool CanBeDeleted { get; set; }
    public AssetHolderType AssetHolderType { get; set; }
}