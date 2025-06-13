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

    [Required, MaxLength(30)]
    public string InputForTransactions { get; set; } = string.Empty;

    [Precision(18, 2)]
    public decimal? DefaultRakeCommission { get; set; }

    [Precision(18, 2)]
    public decimal? DefaultParentCommission { get; set; }


    public Guid WalletId { get; set; }
    public virtual Wallet Wallet { get; set; } = new Wallet();

    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }

    public Guid? MemberId { get; set; }
    public virtual Member? Member { get; set; }

    public Guid? BankId { get; set; }
    public virtual Bank? Bank { get; set; }

    public Guid? PokerManagerId { get; set; }
    public virtual PokerManager? PokerManager { get; set; }
}