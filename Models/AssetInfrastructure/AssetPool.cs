using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.AssetInfrastructure;

public class AssetPool : BaseDomain
{
    // An asset pool with no asset holder belongs to the company
    public Guid? BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public AssetGroup AssetGroup { get; set; }
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new HashSet<WalletIdentifier>();
}