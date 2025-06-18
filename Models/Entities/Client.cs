using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Client : BaseAssetHolder
{
    public DateTime? Birthday { get; set; }

    public virtual ICollection<ContactPhone> ContactPhones { get; set; } = new HashSet<ContactPhone>();

    public virtual ICollection<InitialBalance> InitialBalances { get; set; } = new HashSet<InitialBalance>();
}