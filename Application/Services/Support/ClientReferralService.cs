using Microsoft.EntityFrameworkCore;
using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.Support;
using SFManagement.Infrastructure.Data;

namespace SFManagement.Application.Services.Support;

/// <summary>
/// Service for managing Client-specific referrals
/// Handles referral creation, validation, and management for Client WalletIdentifiers
/// </summary>
public class ClientReferralService
{
    private readonly DataContext _context;
    private readonly ReferralService _referralService;
    
    public ClientReferralService(DataContext context, ReferralService referralService)
    {
        _context = context;
        _referralService = referralService;
    }
    
    /// <summary>
    /// Creates a referral for a Client's WalletIdentifier
    /// Validates that both referrer and referred are Clients
    /// </summary>
    public async Task<Referral> CreateClientReferral(Guid referrerClientId, Guid walletIdentifierId, decimal commissionPercentage, DateTime? activeFrom = null, DateTime? activeUntil = null)
    {
        // Validate referrer is a Client
        var referrerClient = await _context.Clients
            .Include(c => c.BaseAssetHolder)
            .FirstOrDefaultAsync(c => c.BaseAssetHolderId == referrerClientId && !c.DeletedAt.HasValue);
        
        if (referrerClient == null)
            throw new ArgumentException($"Referrer Client not found: {referrerClientId}");
        
        // Validate wallet identifier belongs to a Client
        var walletIdentifier = await _context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(ap => ap.BaseAssetHolder)
                .ThenInclude(bah => bah.Client)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);
        
        if (walletIdentifier == null)
            throw new ArgumentException($"WalletIdentifier not found: {walletIdentifierId}");
        
        if (walletIdentifier.AssetPool?.BaseAssetHolder?.Client == null)
            throw new ArgumentException("WalletIdentifier does not belong to a Client");
        
        // Use the base ReferralService to create the referral
        return await _referralService.CreateReferral(referrerClientId, walletIdentifierId, commissionPercentage, activeFrom, activeUntil);
    }
    
    /// <summary>
    /// Gets all referrals made by a Client
    /// </summary>
    public async Task<List<Referral>> GetClientReferralsMade(Guid clientId)
    {
        // Validate client exists
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.BaseAssetHolderId == clientId && !c.DeletedAt.HasValue);
        
        if (client == null)
            throw new ArgumentException($"Client not found: {clientId}");
        
        return await _referralService.GetReferralsMadeBy(clientId);
    }
    
    /// <summary>
    /// Gets all referrals received by a Client
    /// </summary>
    public async Task<List<Referral>> GetClientReferralsReceived(Guid clientId)
    {
        // Validate client exists
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.BaseAssetHolderId == clientId && !c.DeletedAt.HasValue);
        
        if (client == null)
            throw new ArgumentException($"Client not found: {clientId}");
        
        return await _referralService.GetReferralsReceivedBy(clientId);
    }
    
    /// <summary>
    /// Gets active referral for a Client's WalletIdentifier at a specific date
    /// </summary>
    public async Task<Referral?> GetActiveReferralForClientWallet(Guid walletIdentifierId, DateTime? atDate = null)
    {
        // Validate wallet belongs to a Client
        var walletIdentifier = await _context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(ap => ap.BaseAssetHolder)
                .ThenInclude(bah => bah.Client)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);
        
        if (walletIdentifier?.AssetPool?.BaseAssetHolder?.Client == null)
            throw new ArgumentException("WalletIdentifier does not belong to a Client");
        
        return await _referralService.GetActiveReferralForWallet(walletIdentifierId, atDate);
    }
    
    /// <summary>
    /// Deactivates active referral for a Client's WalletIdentifier at a specific date
    /// </summary>
    public async Task<bool> DeactivateClientWalletReferral(Guid walletIdentifierId, DateTime deactivationDate)
    {
        // Validate wallet belongs to a Client
        var walletIdentifier = await _context.WalletIdentifiers
            .Include(wi => wi.AssetPool)
                .ThenInclude(ap => ap.BaseAssetHolder)
                .ThenInclude(bah => bah.Client)
            .FirstOrDefaultAsync(wi => wi.Id == walletIdentifierId && !wi.DeletedAt.HasValue);
        
        if (walletIdentifier?.AssetPool?.BaseAssetHolder?.Client == null)
            throw new ArgumentException("WalletIdentifier does not belong to a Client");
        
        return await _referralService.DeactivateActiveReferral(walletIdentifierId, deactivationDate);
    }
    
    /// <summary>
    /// Deactivates active referral for a Client's WalletIdentifier immediately
    /// </summary>
    public async Task<bool> DeactivateClientWalletReferralNow(Guid walletIdentifierId)
    {
        return await DeactivateClientWalletReferral(walletIdentifierId, DateTime.UtcNow);
    }
    
    /// <summary>
    /// Gets potential referrers (other Clients) for a Client
    /// </summary>
    public async Task<List<Client>> GetPotentialReferrers(Guid clientId)
    {
        return await _context.Clients
            .Include(c => c.BaseAssetHolder)
            .Where(c => c.BaseAssetHolderId != clientId && !c.DeletedAt.HasValue)
            .ToListAsync();
    }
    
    /// <summary>
    /// Gets referral statistics for a Client
    /// </summary>
    public async Task<ClientReferralStatistics> GetClientReferralStatistics(Guid clientId)
    {
        var referralsMade = await GetClientReferralsMade(clientId);
        var referralsReceived = await GetClientReferralsReceived(clientId);
        
        var activeReferralsMade = referralsMade.Where(r => r.IsActive).ToList();
        var activeReferralsReceived = referralsReceived.Where(r => r.IsActive).ToList();
        
        return new ClientReferralStatistics
        {
            ClientId = clientId,
            TotalReferralsMade = referralsMade.Count,
            ActiveReferralsMade = activeReferralsMade.Count,
            TotalReferralsReceived = referralsReceived.Count,
            ActiveReferralsReceived = activeReferralsReceived.Count,
            TotalCommissionEarned = activeReferralsMade.Sum(r => r.ParentCommission ?? 0),
            UniqueClientsReferred = referralsMade
                .Where(r => r.WalletIdentifier?.AssetPool?.BaseAssetHolderId != null)
                .Select(r => r.WalletIdentifier!.AssetPool!.BaseAssetHolderId)
                .Distinct()
                .Count()
        };
    }
}

/// <summary>
/// Client referral statistics data transfer object
/// </summary>
public class ClientReferralStatistics
{
    public Guid ClientId { get; set; }
    public int TotalReferralsMade { get; set; }
    public int ActiveReferralsMade { get; set; }
    public int TotalReferralsReceived { get; set; }
    public int ActiveReferralsReceived { get; set; }
    public decimal TotalCommissionEarned { get; set; }
    public int UniqueClientsReferred { get; set; }
} 