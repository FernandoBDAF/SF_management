using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Closing;

public class ClosingWallet : BaseDomain
{
    public ClosingWallet()
    {
    }

    // public ClosingWallet(AssetWallet assetWallet)
    // {
    //     WalletId = assetWallet.Id;
    //     ReturnRake = decimal.Zero;
    // }

    public Guid ClosingManagerId { get; set; }
    
    public virtual ClosingManager ClosingManager { get; set; }

    public Guid WalletId { get; set; }
    
    public virtual AssetWallet AssetWallet { get; set; }

    [Precision(18, 2)] public decimal ReturnRake { get; set; }
}