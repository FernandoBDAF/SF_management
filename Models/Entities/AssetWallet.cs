using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class AssetWallet : BaseDomain
{
    public AssetType AssetType { get; set; }
    
    public virtual ICollection<FiatAssetTransaction>? FiatAssetTransactions { get; set; }
    public virtual ICollection<DigitalAssetTransaction>? DigitalAssetTransactions { get; set; }
    
    // Only one of the following relationships bellow should happen, so there is
    // a logic handling this in the service.
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
    
    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }
    
    public Guid? BankId { get; set; }
    public virtual Bank? Bank { get; set; }
    
    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}