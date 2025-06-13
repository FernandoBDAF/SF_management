using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Closing;

public class ClosingWallet : BaseDomain
{
    public ClosingWallet()
    {
    }

    // public ClosingWallet(Wallet wallet)
    // {
    //     WalletId = wallet.Id;
    //     ReturnRake = decimal.Zero;
    // }

    public Guid ClosingManagerId { get; set; }
    
    public virtual ClosingManager ClosingManager { get; set; }

    public Guid WalletId { get; set; }
    
    public virtual Wallet Wallet { get; set; }

    [Precision(18, 2)] public decimal ReturnRake { get; set; }
}