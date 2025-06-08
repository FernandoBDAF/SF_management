using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models;

public class AvgRate : BaseDomain
{
    public DateTime Date { get; set; }

    [Precision(18, 2)] public decimal Value { get; set; } = decimal.Zero;

    [ForeignKey("Manager")] public Guid ManagerId { get; set; }

    public virtual Manager Manager { get; set; }
}