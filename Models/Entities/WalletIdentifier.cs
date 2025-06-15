using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Closing;

namespace SFManagement.Models.Entities;

public class WalletIdentifier : BaseDomain
{
    [Required, MaxLength(30)]
    public string? Nickname { get; set; } = string.Empty;

    [MaxLength(40)]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string? Phone { get; set; }

    [MaxLength(50)]
    public string? Pix { get; set; }
    
    // [MaxLength(20)]
    // public string? Agency { get; set; }
    //
    // [MaxLength(50)]
    // public string? Account { get; set; }
    

    [Required, MaxLength(30)]
    public string InputForTransactions { get; set; } = string.Empty;

    [Precision(18, 2)]
    public decimal? DefaultRakeCommission { get; set; }

    [Precision(18, 2)]
    public decimal? DefaultParentCommission { get; set; }

    // Aiming to avoid dependency loop it is necessary to handle the deletion of
    // the 5 relationships bellow in the datacontext file. On delete of assetWallet,
    // WalletIdentifier will be deleted. Deleting any of the other classes will
    // leave and orphan assetWallet identifier 
    public Guid AssetWalletId { get; set; }
    public virtual AssetWallet AssetWallet { get; set; } = new AssetWallet();
    
    // Only one of the following relationships bellow should happen, so there is
    // a logic handling this in the service.
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }

    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}