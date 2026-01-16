using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Entities.Transactions;

namespace SFManagement.Domain.Entities.AssetHolders;

public class BaseAssetHolder : BaseDomain
{
    [Required] [MaxLength(32)] public string Name { get; set; }

    [Required] public TaxEntityType TaxEntityType { get; set; }
    
    [Required] [MaxLength(20)] [Column(TypeName = "varchar(20)")] public string GovernmentNumber { get; set; }
    
    // Navigation properties to specific asset holder types (only one will have a value)
    public virtual Client? Client { get; set; }
    public virtual Bank? Bank { get; set; }
    public virtual Member? Member { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
    
    // Computed property to get the specific asset holder type
    public object? SpecificAssetHolder
    {
        get
        {
            if (Client != null) return Client;
            if (Bank != null) return Bank;
            if (Member != null) return Member;
            if (PokerManager != null) return PokerManager;
            return null;
        }
    }
    
    // Property to get the asset holder type as an enum
    public AssetHolderType AssetHolderType
    {
        get
        {
            if (Client != null) return AssetHolderType.Client;
            if (Bank != null) return AssetHolderType.Bank;
            if (Member != null) return AssetHolderType.Member;
            if (PokerManager != null) return AssetHolderType.PokerManager;
            return AssetHolderType.Unknown;
        }
    }
    
    /// <summary>
    /// Validates that exactly one entity type is set (mutually exclusive)
    /// </summary>
    [NotMapped]
    public bool HasSingleEntityType => 
        (Client != null ? 1 : 0) + (Bank != null ? 1 : 0) + 
        (Member != null ? 1 : 0) + (PokerManager != null ? 1 : 0) == 1;
    
    public virtual ICollection<AssetPool> AssetPools { get; set; } = new HashSet<AssetPool>();
    
    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();

    public virtual ICollection<Address> Addresses { get; set; } = new HashSet<Address>();
    
    public virtual ICollection<ContactPhone> ContactPhones { get; set; } = new HashSet<ContactPhone>();

    /// <summary>
    /// Transactions imported from external files (OFX, Excel, etc.) for this asset holder
    /// </summary>
    public virtual ICollection<ImportedTransaction> ImportedTransactions { get; set; } = new HashSet<ImportedTransaction>();

    /// <summary>
    /// The BaseAssetHolder who referred this one (optional)
    /// Frontend should provide this ID when creating a referred BaseAssetHolder
    /// </summary>
    public Guid? ReferrerId { get; set; }
    public virtual BaseAssetHolder? Referrer { get; set; }

    /// <summary>
    /// Referrals made BY this BaseAssetHolder (this asset holder is the referrer)
    /// These are the WalletIdentifiers that this asset holder has referred
    /// </summary>
    public virtual ICollection<Referral> ReferralsMade { get; set; } = new HashSet<Referral>();
    
    /// <summary>
    /// Helper property to get referrals received by this BaseAssetHolder
    /// These are referrals where this asset holder's WalletIdentifiers are being referred by others
    /// </summary>
    public IEnumerable<Referral> ReferralsReceived => 
        AssetPools?.SelectMany(ap => ap.WalletIdentifiers)
                  ?.SelectMany(wi => wi.Referrals) ?? Enumerable.Empty<Referral>();
}