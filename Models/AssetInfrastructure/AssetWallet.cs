using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.AssetInfrastructure;

public class AssetWallet : BaseDomain
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }
    
    public AssetType AssetType { get; set; }
    
    public virtual ICollection<WalletIdentifier> WalletIdentifiers { get; set; } = new HashSet<WalletIdentifier>();
}