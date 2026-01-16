using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.AssetHolders;

namespace SFManagement.Domain.Entities.Support;

/// <summary>
/// Represents a referral relationship between a BaseAssetHolder (referrer) and a WalletIdentifier (referred)
/// This allows tracking commission-based referrals for specific wallet identifiers
/// </summary>
public class Referral : BaseDomain
{
    /// <summary>
    /// The BaseAssetHolder who made the referral (the referrer)
    /// Maps to AssetHolderId in database
    /// </summary>
    [Required] 
    public Guid AssetHolderId { get; set; }
    public virtual BaseAssetHolder? AssetHolder { get; set; }

    /// <summary>
    /// The WalletIdentifier being referred (owned by the referred BaseAssetHolder)
    /// This creates a commission relationship for transactions involving this wallet
    /// </summary>
    [Required] 
    public Guid WalletIdentifierId { get; set; }
    public virtual WalletIdentifier? WalletIdentifier { get; set; }
    
    /// <summary>
    /// Commission percentage that the referrer receives (0-100)
    /// Maps to ParentCommission in database
    /// </summary>
    [Precision(18, 4)] [Range(0, 100)] public decimal? ParentCommission { get; set; }
    
    /// <summary>
    /// Date when the referral starts being active
    /// If null, the referral was always active (from the beginning of time)
    /// </summary>
    public DateTime? ActiveFrom { get; set; }
    
    /// <summary>
    /// Date when the referral commission expires
    /// If null, the referral never expires
    /// Maps to ActiveUntil in database
    /// </summary>
    public DateTime? ActiveUntil { get; set; }
    
    /// <summary>
    /// Helper property to check if referral is currently active
    /// </summary>
    public bool IsActive => 
        (ActiveFrom == null || DateTime.UtcNow >= ActiveFrom) &&
        (ActiveUntil == null || DateTime.UtcNow <= ActiveUntil) && 
        !DeletedAt.HasValue;
    
    /// <summary>
    /// Helper method to check if referral is active at a specific date
    /// </summary>
    public bool IsActiveAt(DateTime date) =>
        (ActiveFrom == null || date >= ActiveFrom) &&
        (ActiveUntil == null || date <= ActiveUntil) && 
        !DeletedAt.HasValue;
    
    /// <summary>
    /// Gets the BaseAssetHolder who owns the referred WalletIdentifier
    /// </summary>
    public BaseAssetHolder? ReferredAssetHolder => WalletIdentifier?.AssetPool?.BaseAssetHolder;
}