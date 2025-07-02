namespace SFManagement.Models.Entities;

public class Referral
{
    public Guid? WalletIdentifierId { get; set; }
    public virtual WalletIdentifier? WalletIdentifier { get; set; }
}