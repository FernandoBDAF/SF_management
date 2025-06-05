using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.Models;

public class Nickname : BaseDomain
{
    public string Name { get; set; }

    [ForeignKey("Wallet")] public Guid WalletId { get; set; }

    public virtual Wallet Wallet { get; set; }

    [ForeignKey("Client")] public Guid ClientId { get; set; }

    public virtual Client Client { get; set; }

    public virtual List<ClosingNickname> ClosingNicknames { get; set; } = new();
}