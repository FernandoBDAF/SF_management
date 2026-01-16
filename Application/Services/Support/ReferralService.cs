using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.Support;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Support;

public class ReferralService : BaseService<Referral>
{
    public ReferralService(DataContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
    }

    /// <summary>
    /// Creates a referral for a specific WalletIdentifier
    /// Only one active referral per WalletIdentifier is allowed
    /// </summary>
    public async Task<Referral> CreateReferral(Guid referrerAssetHolderId, Guid walletIdentifierId, decimal commissionPercentage, DateTime? activeFrom = null, DateTime? activeUntil = null)
    {
        // Validate referrer exists
        var referrer = await context.BaseAssetHolders
            .FirstOrDefaultAsync(bah => bah.Id == referrerAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (referrer == null)
            throw new ArgumentException($"Referrer BaseAssetHolder not found: {referrerAssetHolderId}");
        
        // Validate wallet identifier exists
        var walletIdentifier = await context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);
        
        if (walletIdentifier == null)
            throw new ArgumentException($"WalletIdentifier not found: {walletIdentifierId}");
        
        // Prevent self-referral
        if (walletIdentifier.AssetPool!.BaseAssetHolderId == referrerAssetHolderId)
            throw new ArgumentException("Cannot create self-referral");
        
        // Set default activeFrom to now if not provided
        var referralActiveFrom = activeFrom ?? DateTime.UtcNow;
        
        // Check for existing active referral and deactivate it
        var existingActiveReferral = await context.Referrals
            .FirstOrDefaultAsync(r => r.WalletIdentifierId == walletIdentifierId && 
                                     (r.ActiveFrom == null || r.ActiveFrom <= referralActiveFrom) &&
                                     (r.ActiveUntil == null || r.ActiveUntil > referralActiveFrom) &&
                                     !r.DeletedAt.HasValue);
        
        if (existingActiveReferral != null)
        {
            // Deactivate existing referral by setting ActiveUntil to the day before the new referral starts
            existingActiveReferral.ActiveUntil = referralActiveFrom.AddDays(-1);
            existingActiveReferral.UpdatedAt = DateTime.UtcNow;
        }
        
        var referral = new Referral
        {
            AssetHolderId = referrerAssetHolderId,
            WalletIdentifierId = walletIdentifierId,
            ParentCommission = commissionPercentage,
            ActiveFrom = referralActiveFrom,
            ActiveUntil = activeUntil
        };
        
        return await Add(referral);
    }
    
    /// <summary>
    /// Gets all referrals made by a BaseAssetHolder
    /// </summary>
    public async Task<List<Referral>> GetReferralsMadeBy(Guid assetHolderId)
    {
        return await context.Referrals
            .Include(r => r.AssetHolder)
            .Include(r => r.WalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
                .ThenInclude(ap => ap.BaseAssetHolder)
            .Where(r => r.AssetHolderId == assetHolderId && !r.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets all referrals received by a BaseAssetHolder
    /// </summary>
    public async Task<List<Referral>> GetReferralsReceivedBy(Guid assetHolderId)
    {
        return await context.Referrals
            .Include(r => r.AssetHolder)
            .Include(r => r.WalletIdentifier)
                .ThenInclude(wi => wi!.AssetPool)
                .ThenInclude(ap => ap!.BaseAssetHolder)
            .Where(r => r.WalletIdentifier!.AssetPool!.BaseAssetHolderId == assetHolderId && !r.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets the active referral for a specific WalletIdentifier at a given date
    /// </summary>
    public async Task<Referral?> GetActiveReferralForWallet(Guid walletIdentifierId, DateTime? atDate = null)
    {
        var checkDate = atDate ?? DateTime.UtcNow;
        return await context.Referrals
            .Include(r => r.AssetHolder)
            .Include(r => r.WalletIdentifier)
            .FirstOrDefaultAsync(r => r.WalletIdentifierId == walletIdentifierId && 
                                     (r.ActiveFrom == null || r.ActiveFrom <= checkDate) &&
                                     (r.ActiveUntil == null || r.ActiveUntil > checkDate) && 
                                     !r.DeletedAt.HasValue);
    }
    
    /// <summary>
    /// Deactivates the current active referral for a WalletIdentifier at a specific date
    /// </summary>
    public async Task<bool> DeactivateActiveReferral(Guid walletIdentifierId, DateTime deactivationDate)
    {
        var activeReferral = await GetActiveReferralForWallet(walletIdentifierId, deactivationDate);
        
        if (activeReferral != null)
        {
            activeReferral.ActiveUntil = deactivationDate;
            activeReferral.UpdatedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Deactivates the current active referral for a WalletIdentifier immediately
    /// </summary>
    public async Task<bool> DeactivateActiveReferralNow(Guid walletIdentifierId)
    {
        return await DeactivateActiveReferral(walletIdentifierId, DateTime.UtcNow);
    }
} 