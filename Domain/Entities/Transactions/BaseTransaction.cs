using SFManagement.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Metadata;
using SFManagement.Domain.Entities.Assets;
using SFManagement.Domain.Entities.Support;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Domain.Entities.Transactions;

public abstract class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }
    
    public Guid? CategoryId { get; set; }
    public virtual Category? Category { get; set; }

    // Sender
    [Required] public Guid SenderWalletIdentifierId { get; set; }
    public virtual WalletIdentifier? SenderWalletIdentifier { get; set; }
    
    // Receiver
    [Required] public Guid ReceiverWalletIdentifierId { get; set; }
    public virtual WalletIdentifier? ReceiverWalletIdentifier { get; set; }
    
    // only positive amounts are allowed
    [Required] [Precision(18, 2)] public decimal AssetAmount { get; set; }
    
    public DateTime? ApprovedAt { get; set; }

    // Helper methods for transaction direction and counterparty
    public bool IsReceiver(Guid walletIdentifierId) => ReceiverWalletIdentifierId == walletIdentifierId;

    public bool IsSender(Guid walletIdentifierId) => SenderWalletIdentifierId == walletIdentifierId;


    /*
    invoiceID?
    transactionID?
    */

    /*
    put transaction
    */

    public WalletIdentifier GetCounterpartyForWalletIdentifier(Guid walletIdentifierId)
    {
        if (SenderWalletIdentifierId == walletIdentifierId)
            return ReceiverWalletIdentifier!;
        
        if (ReceiverWalletIdentifierId == walletIdentifierId)
            return SenderWalletIdentifier!;
            
        throw new ArgumentException("Wallet identifier is not involved in this transaction");
    }


    public decimal GetSignedAmountForWalletIdentifier(Guid walletIdentifierId)
    {
        if (SenderWalletIdentifierId == walletIdentifierId)
            return -AssetAmount; // Outgoing (negative)
        
        if (ReceiverWalletIdentifierId == walletIdentifierId)
            return AssetAmount; // Incoming (positive)
            
        throw new ArgumentException("Wallet identifier is not involved in this transaction");
    }

    public bool HaveBothWalletsSameAccountClassification()
    {
        return SenderWalletIdentifier!.AccountClassification == ReceiverWalletIdentifier!.AccountClassification;
    }

    public bool IsWalletIdentifierLiability(Guid walletIdentifierId)
    {
        if (SenderWalletIdentifierId == walletIdentifierId)
            return SenderWalletIdentifier!.AccountClassification == AccountClassification.LIABILITY;
        
        return ReceiverWalletIdentifier!.AccountClassification == AccountClassification.LIABILITY;
    }

    [NotMapped]
    public bool IsInternalTransfer => SenderWalletIdentifier?.AssetPool?.BaseAssetHolderId == 
                                     ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolderId;

    public string GetCounterPartyName(Guid walletIdentifierId)
    {
        if (SenderWalletIdentifierId == walletIdentifierId)
            return ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown";
        
        if (ReceiverWalletIdentifierId == walletIdentifierId)
            return SenderWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown";
            
        throw new ArgumentException("Wallet identifier is not involved in this transaction");
    }

    public string GetWalletIdentifierInput(Guid walletIdentifierId)
    {
        if (SenderWalletIdentifierId == walletIdentifierId)
            return SenderWalletIdentifier?.GetPokerMetadata(PokerWalletMetadata.InputForTransactions) ?? "";
        
        if (ReceiverWalletIdentifierId == walletIdentifierId)
            return ReceiverWalletIdentifier?.GetPokerMetadata(PokerWalletMetadata.InputForTransactions) ?? "";
            
        throw new ArgumentException("Wallet identifier is not involved in this transaction");
    }
}