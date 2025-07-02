using System.ComponentModel.DataAnnotations;
using SFManagement.Enums;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.AssetInfrastructure;

public class AssetWallet : BaseDomain
{
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder BaseAssetHolder { get; set; }
    
    public AssetType AssetType { get; set; }
    
    public virtual ICollection<FiatAssetTransaction>? FiatAssetTransactions { get; set; }
    
    public virtual ICollection<DigitalAssetTransaction>? DigitalAssetTransactions { get; set; }
    
    public virtual ICollection<SettlementTransaction> SettlementTransactions { get; set; }
}