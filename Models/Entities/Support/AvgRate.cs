using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models.Entities;

public class AvgRate : BaseDomain
{
    public DateTime Date { get; set; }

    [Precision(18, 2)] public decimal Value { get; set; } = decimal.Zero;

    // public Guid PokerManagerId { get; set; }
    public Guid PokerManagerId { get; set; }
}