using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Exceptions;
using SFManagement.Interfaces;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models;
using SFManagement.Models.AssetInfrastructure;

namespace SFManagement.Services;

public class BaseAssetHolderService<TEntity>(DataContext context, IHttpContextAccessor httpContextAccessor, IAssetHolderDomainService domainService) 
    : BaseService<TEntity>(context, httpContextAccessor) where TEntity : BaseDomain, IAssetHolder
{
    protected readonly IAssetHolderDomainService _domainService = domainService;

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
        
        return await query.Where(x => !x.BaseAssetHolder.DeletedAt.HasValue)
        .OrderByDescending(x => x.BaseAssetHolder.CreatedAt)
        .ToListAsync();
    }

    public override async Task<TEntity?> Get(Guid id)
    {
        // Get the BaseAssetHolder first since the ID is always from BaseAssetHolder
        var baseAssetHolder = await context.BaseAssetHolders
            .Include(c => c.AssetWallets)
                .ThenInclude(aw => aw.WalletIdentifiers)
            .Include(c => c.Address)
            .Include(c => c.ContactPhones)
            .Include(c => c.InitialBalances)
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
                request.Email, 
                request.Cpf, 
                request.Cnpj
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
                baseAssetHolder.Email = request.Email;
                baseAssetHolder.Cpf = request.Cpf;
                baseAssetHolder.Cnpj = request.Cnpj;
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
            var hasActiveAssetWallets = await _domainService.HasActiveAssetWallets(entity.BaseAssetHolderId);

            return new AssetHolderStatistics
            {
                EntityId = entityId,
                BaseAssetHolderId = entity.BaseAssetHolderId,
                HasActiveTransactions = hasActiveTransactions,
                TotalBalance = totalBalance,
                HasActiveAssetWallets = hasActiveAssetWallets,
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
    public async Task<Guid[]> GetAssetHolderAssetWalletIds()
    {
        var assetHolderType = GetAssetHolderTypeForEntity();
        var assetWalletIds = await context.BaseAssetHolders
            .Where(bah => bah.AssetHolderType == assetHolderType)
            .Include(bah => bah.AssetWallets)
            .SelectMany(bah => bah.AssetWallets.Where(aw => !aw.DeletedAt.HasValue).Select(aw => aw.Id))
            .ToArrayAsync();
        
        return assetWalletIds;
    }
    
    public async Task<List<BaseAssetHolder>> GetFilteredByWalletIdentifierType(AssetType assetType)
    {
        // Build query based on the specific entity type using strategy pattern
        IQueryable<BaseAssetHolder> query = context.BaseAssetHolders;
        
        if (FilterStrategies.TryGetValue(typeof(TEntity), out var filterStrategy))
        {
            query = filterStrategy(query);
        }
        
        var baseAssetHolders = await query
            .Include(bah => bah.AssetWallets)
                .ThenInclude(aw => aw.WalletIdentifiers)
            .Where(bah => bah.AssetWallets.Any(aw => 
                aw.WalletIdentifiers.Any(wi => wi.AssetWallet.AssetType == assetType && !wi.DeletedAt.HasValue)))
            .ToListAsync();

        return baseAssetHolders;
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

    // this method may be removed
    public class AssetBalance
    {
        public AssetType AssetType { get; set; }
        public decimal? Value { get; set; }
    }

    public async Task<Dictionary<AssetType, decimal>> GetBalancesByAssetType(Guid baseAssetHolderId)
    {
        var balances = new Dictionary<AssetType, decimal>();

        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
            .Where(wi => wi.AssetWallet.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
            .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
       
        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .ToArrayAsync() ?? [];

        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .ToArrayAsync() ?? [];

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
            .ToArrayAsync() ?? [];

        // Process digital asset transactions using helper methods
        foreach (var tx in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var assetType = tx.IsReceiver(relevantWalletId) ? 
                tx.ReceiverWalletIdentifier.AssetWallet.AssetType : 
                tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        // Process fiat asset transactions using helper methods
        foreach (var tx in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var assetType = tx.IsReceiver(relevantWalletId) ? 
                tx.ReceiverWalletIdentifier.AssetWallet.AssetType : 
                tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            
            if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        // Process settlement transactions using helper methods
        foreach (var tx in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                tx.SenderWalletIdentifierId == id || tx.ReceiverWalletIdentifierId == id);
            
            var assetType = tx.IsReceiver(relevantWalletId) ? 
                tx.ReceiverWalletIdentifier.AssetWallet.AssetType : 
                tx.SenderWalletIdentifier.AssetWallet.AssetType;
            
            var signedAmount = tx.GetSignedAmountForWalletIdentifier(relevantWalletId);
            
                if (!balances.ContainsKey(assetType)) balances[assetType] = 0;
            balances[assetType] += signedAmount;
        }

        return balances;
    }
    
    // public async Task<BaseAssetHolder> GetAssetHolderWithTransactionsNoCascade(Guid baseAssetHolderId)
    // {
    //     // Use a completely different approach with projection to avoid cascade
    //     var query = context.BaseAssetHolders.AsQueryable();
        
    //     // First, get the asset holder with basic info
    //     var assetHolder = await query
    //         .Include(c => c.AssetWallets)
    //         .Include(c => c.WalletIdentifiers)
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(x => x.Id == baseAssetHolderId) ?? throw new Exception("AssetHolder not found");

    //     // Now load transactions separately with minimal includes
    //     foreach (var aw in assetHolder.AssetWallets)
    //     {
    //         // Load DigitalAssetTransactions with minimal WalletIdentifier info
    //         aw.DigitalAssetTransactions = await context.DigitalAssetTransactions
    //             .Where(dat => dat.AssetWalletId == aw.Id && !dat.DeletedAt.HasValue)
    //             .Select(dat => new DigitalAssetTransaction
    //             {
    //                 Id = dat.Id,
    //                 Date = dat.Date,
    //                 Description = dat.Description,
    //                 AssetAmount = dat.AssetAmount,
    //                 TransactionDirection = dat.TransactionDirection,
    //                 BalanceAs = dat.BalanceAs,
    //                 ConversionRate = dat.ConversionRate,
    //                 Rate = dat.Rate,
    //                 AssetWalletId = dat.AssetWalletId,
    //                 WalletIdentifierId = dat.WalletIdentifierId,
    //                 WalletIdentifier = new WalletIdentifier
    //                 {
    //                     InputForTransactions = dat.WalletIdentifier.InputForTransactions,
    //                     AssetType = dat.WalletIdentifier.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder
    //                     {
    //                         Id = dat.WalletIdentifier.BaseAssetHolder.Id, 
    //                         Name = dat.WalletIdentifier.BaseAssetHolder.Name
    //                     },
                       
    //                 }
    //             })
    //             .ToListAsync();

    //         // Load FiatAssetTransactions with minimal WalletIdentifier info
    //         aw.FiatAssetTransactions = await context.FiatAssetTransactions
    //             .Where(fat => fat.AssetWalletId == aw.Id && !fat.DeletedAt.HasValue)
    //             .Select(fat => new FiatAssetTransaction
    //             {
    //                 Id = fat.Id,
    //                 Date = fat.Date,
    //                 Description = fat.Description,
    //                 AssetAmount = fat.AssetAmount,
    //                 TransactionDirection = fat.TransactionDirection,
    //                 AssetWalletId = fat.AssetWalletId,
    //                 WalletIdentifierId = fat.WalletIdentifierId,
    //                 WalletIdentifier = new WalletIdentifier
    //                 {
    //                     Id = fat.WalletIdentifier.Id,
    //                     InputForTransactions = fat.WalletIdentifier.InputForTransactions,
    //                     AssetType = fat.WalletIdentifier.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder 
    //                         { 
    //                             Id = fat.WalletIdentifier.BaseAssetHolder.Id,
    //                             Name = fat.WalletIdentifier.BaseAssetHolder.Name 
    //                         },
    //                 }
    //             })
    //             .ToListAsync();
    //     }

    //     foreach (var wi in assetHolder.WalletIdentifiers)
    //     {
    //         // Load DigitalAssetTransactions with minimal AssetWallet info
    //         wi.DigitalAssetTransactions = await context.DigitalAssetTransactions
    //             .Where(dat => dat.WalletIdentifierId == wi.Id && !dat.DeletedAt.HasValue)
    //             .Select(dat => new DigitalAssetTransaction
    //             {
    //                 Id = dat.Id,
    //                 Date = dat.Date,
    //                 Description = dat.Description,
    //                 AssetAmount = dat.AssetAmount,
    //                 TransactionDirection = dat.TransactionDirection,
    //                 BalanceAs = dat.BalanceAs,
    //                 ConversionRate = dat.ConversionRate,
    //                 Rate = dat.Rate,
    //                 AssetWalletId = dat.AssetWalletId,
    //                 WalletIdentifierId = dat.WalletIdentifierId,
    //                 AssetWallet = new AssetWallet
    //                 {
    //                     Id = dat.AssetWallet.Id,
    //                     AssetType = dat.AssetWallet.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder
    //                     {
    //                         Id = dat.AssetWallet.BaseAssetHolder.Id, 
    //                         Name = dat.AssetWallet.BaseAssetHolder.Name
    //                     },
    //                 }
    //             })
    //             .ToListAsync();

    //         // Load FiatAssetTransactions with minimal AssetWallet info
    //         wi.FiatAssetTransactions = await context.FiatAssetTransactions
    //             .Where(fat => fat.WalletIdentifierId == wi.Id && !fat.DeletedAt.HasValue)
    //             .Select(fat => new FiatAssetTransaction
    //             {
    //                 Id = fat.Id,
    //                 Date = fat.Date,
    //                 Description = fat.Description,
    //                 AssetAmount = fat.AssetAmount,
    //                 TransactionDirection = fat.TransactionDirection,
    //                 AssetWalletId = fat.AssetWalletId,
    //                 WalletIdentifierId = fat.WalletIdentifierId,
    //                 AssetWallet = new AssetWallet
    //                 {
    //                     Id = fat.AssetWallet.Id,
    //                     AssetType = fat.AssetWallet.AssetType,
    //                     BaseAssetHolder = new BaseAssetHolder
    //                     {
    //                         Id = fat.AssetWallet.BaseAssetHolder.Id, 
    //                         Name = fat.AssetWallet.BaseAssetHolder.Name
    //                     },
    //                 }
    //             })
    //             .ToListAsync();
    //     }

    //     return assetHolder;
    // }

    public async Task<StatementAssetHolderWithTransactions> GetTransactionsStatementForAssetHolder(Guid baseAssetHolderId)
    {
        var walletIdentifiers = await context.WalletIdentifiers
            .Include(wi => wi.AssetWallet)
            .Where(wi => wi.AssetWallet.BaseAssetHolderId == baseAssetHolderId && !wi.DeletedAt.HasValue)
                .ToListAsync();

        var walletIdentifierIds = walletIdentifiers.Select(wi => wi.Id).ToArray();
    
        var digitalTransactions = await context.DigitalAssetTransactions
            .Where(dt => !dt.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(dt.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(dt.ReceiverWalletIdentifierId)))
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];

        var fiatTransactions = await context.FiatAssetTransactions
            .Where(ft => !ft.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(ft.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(ft.ReceiverWalletIdentifierId)))
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];

        var settlementTransactions = await context.SettlementTransactions
            .Where(st => !st.DeletedAt.HasValue && 
                (walletIdentifierIds.Contains(st.SenderWalletIdentifierId) || 
                 walletIdentifierIds.Contains(st.ReceiverWalletIdentifierId)))
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetWallet)
                    .ThenInclude(wi => wi.BaseAssetHolder)
            .ToArrayAsync() ?? [];
        
        var allTransactions = new List<StatementTransactionResponse>();
        
        // Process digital asset transactions using helper methods
        foreach (var dat in digitalTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                dat.SenderWalletIdentifierId == id || dat.ReceiverWalletIdentifierId == id);
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = dat.Id,
                        Date = dat.Date,
                        Description = dat.Description,
                AssetAmount = dat.GetSignedAmountForWalletIdentifier(relevantWalletId),
                        BalanceAs = dat.BalanceAs,
                        ConversionRate = dat.ConversionRate,
                        Rate = dat.Rate,
                AssetType = dat.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = dat.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = dat.GetWalletIdentifierInput(relevantWalletId)
            });
        }

        // Process fiat asset transactions using helper methods
        foreach (var fat in fiatTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                fat.SenderWalletIdentifierId == id || fat.ReceiverWalletIdentifierId == id);
                    
                    allTransactions.Add(new StatementTransactionResponse
                    {
                        Id = fat.Id,
                        Date = fat.Date,
                        Description = fat.Description,
                AssetAmount = fat.GetSignedAmountForWalletIdentifier(relevantWalletId),
                        BalanceAs = null, // Fiat transactions don't have BalanceAs
                        ConversionRate = null, // Fiat transactions don't have ConversionRate
                        Rate = null, // Fiat transactions don't have Rate
                AssetType = fat.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = fat.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = fat.GetWalletIdentifierInput(relevantWalletId)
            });
        }

        // Process settlement transactions using helper methods
        foreach (var st in settlementTransactions)
        {
            var relevantWalletId = walletIdentifierIds.FirstOrDefault(id => 
                st.SenderWalletIdentifierId == id || st.ReceiverWalletIdentifierId == id);
                    
                    allTransactions.Add(new StatementTransactionResponse
            {
                Id = st.Id,
                Date = st.Date,
                Description = st.Description,
                AssetAmount = st.GetSignedAmountForWalletIdentifier(relevantWalletId),
                BalanceAs = null, // Settlement transactions don't have BalanceAs
                ConversionRate = null, // Settlement transactions don't have ConversionRate
                Rate = null, // Settlement transactions don't have Rate
                AssetType = st.SenderWalletIdentifier.AssetWallet.AssetType,
                CounterPartyName = st.GetCounterPartyName(relevantWalletId),
                WalletIdentifierInput = st.GetWalletIdentifierInput(relevantWalletId)
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
    protected async Task<BaseAssetHolder> CreateBaseAssetHolder(string name, string? email = null, string? cpf = null, string? cnpj = null)
    {
        var baseAssetHolder = new BaseAssetHolder
        {
            Name = name,
            Email = email,
            Cpf = cpf,
            Cnpj = cnpj
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
    public bool HasActiveAssetWallets { get; set; }
    public bool CanBeDeleted { get; set; }
    public AssetHolderType AssetHolderType { get; set; }
}