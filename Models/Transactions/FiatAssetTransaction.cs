using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Transactions;

public class FiatAssetTransaction : BaseTransaction
{
    public Guid? OfxTransactionId { get; set; }
    public virtual OfxTransaction? OfxTransaction { get; set; }
    
    [NotMapped]
    public Guid? ClientId { get; set; }
    
    [NotMapped]
    public Guid? BankId { get; set; }
}