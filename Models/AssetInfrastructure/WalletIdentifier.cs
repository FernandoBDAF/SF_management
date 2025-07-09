using System.ComponentModel.DataAnnotations;
using SFManagement.Data;
using SFManagement.Models.Support;
using SFManagement.Models.Transactions;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.AssetInfrastructure;

public class WalletIdentifier : BaseDomain
{
    [Required] public Guid AssetWalletId { get; set; }
    public virtual AssetWallet AssetWallet { get; set; }

    public WalletType WalletType { get; set; }
    
    // Nickname, Routing Number, Agencia
    [MaxLength(40)]
    public string? RouteInfo { get; set; }
    
    // Account Number, email, Conta
    [MaxLength(40)]
    public string? IdentifierInfo { get; set; }
    
    // PIX, PokerManager input, etc...
    [Required, MaxLength(30)]
    public string InputForTransactions { get; set; }
    
    public virtual Referral? Referral { get; set; }

    public IEnumerable<DigitalAssetTransaction> GetDigitalAssetTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.DigitalAssetTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    public IEnumerable<FiatAssetTransaction> GetFiatAssetTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.FiatAssetTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    public IEnumerable<SettlementTransaction> GetSettlementTransactions(DataContext context, bool includeDeleted = false)
    {
        var transactions = context.SettlementTransactions
        .Where(x => x.SenderWalletIdentifierId == Id || x.ReceiverWalletIdentifierId == Id && (!x.DeletedAt.HasValue || includeDeleted))
        .Include(x => x.SenderWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .Include(x => x.ReceiverWalletIdentifier)
            .ThenInclude(x => x.AssetWallet)
                .ThenInclude(x => x.BaseAssetHolder)
        .ToArray();

        return transactions;
    }
    
    // Removed transaction navigation properties as transactions now reference WalletIdentifiers as sender/receiver
    // Navigation properties would be complex to maintain and could be confusing
    // To get transactions for a wallet identifier, query BaseTransaction where 
    // SenderWalletIdentifierId = this.Id OR ReceiverWalletIdentifierId = this.Id
}