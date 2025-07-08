using SFManagement.Enums;
using SFManagement.Models.Entities;

namespace SFManagement.Models.Transactions;

public class AgencyInvoice : BaseDomain
{
    public Guid Id { get; set; }

    public Guid? FiatTransactionId { get; set; }

    public Guid? ReminderId { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? ExternalId { get; set; }

    public Guid? BaseAssetHolderId { get; set; }

    public virtual BaseAssetHolder GetInvoiceSubject()
    {
        return new BaseAssetHolder
        {
            Id = Guid.NewGuid(),
            Name = "Agency",
            TaxEntityType = TaxEntityType.CNPJ_Not_Taxable
        };
    }

    public virtual decimal GetProfit()
    {
        var profit = DigitalAssetTransactions.Sum(x => x.TransactionDirection == TransactionDirection.Income ? 
        x.AssetAmount * x.ConversionRate : -x.AssetAmount * x.ConversionRate);
        return profit ?? 0;
    }

    public virtual ICollection<DigitalAssetTransaction> DigitalAssetTransactions { get; set; } = new HashSet<DigitalAssetTransaction>();
}