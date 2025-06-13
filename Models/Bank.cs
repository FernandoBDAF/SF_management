using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Models.Transactions;

namespace SFManagement.Models.Entities;

public class Bank : BaseAssetHolder
{
    [Required] public int Code { get; set; }
    
    public virtual ICollection<Ofx> Ofx { get; set; } = new List<Ofx>();
}