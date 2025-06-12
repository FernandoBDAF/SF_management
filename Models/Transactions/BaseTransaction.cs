using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models.Transactions;

public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }

    [Required][Precision(18, 2)] public decimal Value { get; set; }

    [ForeignKey("Client")] public Guid? ClientId { get; set; }

    public virtual Client? Client { get; set; } = new();
    
    [ForeignKey("Member")] public Guid? MemberId { get; set; }

    public virtual Member? Member { get; set; } = new();

    [ForeignKey("Manager")] public Guid? ManagerId { get; set; }

    public virtual Manager? Manager { get; set; } = new();

    [MaxLength(30)]
    public string? Description { get; set; }

    [ForeignKey("Tag")] public Guid? TagId { get; set; }

    public virtual Tag Tag { get; set; } = new();
    
    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}