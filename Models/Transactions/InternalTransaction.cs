using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Closing;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Transactions;
// This transaction might be deleted with the new flow 
public class InternalTransaction : BaseTransaction
{
    [Precision(18, 2)] public decimal? Value { get; set; }

    [Precision(18, 2)] public decimal? Coins { get; set; }

    [Precision(18, 2)] public decimal? ExchangeRate { get; set; }

    public TransactionDirection InternalTransactionType { get; set; }

    public Guid? TransferId { get; set; }

    public Guid? BankId { get; set; }
    public Bank? Bank { get; set; }

    public Guid? ClosingManagerId { get; set; }
    public virtual ClosingManager? ClosingManager { get; set; }

    public bool IsProfit { get; set; }
}