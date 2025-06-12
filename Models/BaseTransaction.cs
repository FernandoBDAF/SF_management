using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SFManagement.Models.Transactions;

// This class defines operations that transfer money between a central operator and a user of the system
// Transactions impact the balance of a asset of one or more objects
public class BaseTransaction : BaseDomain
{
    [Required] public DateTime Date { get; set; }

    [ForeignKey("Client")] public Guid? ClientId { get; set; }

    public virtual Client? Client { get; set; } = new();
    
    [ForeignKey("Member")] public Guid? MemberId { get; set; }

    public virtual Member? Member { get; set; } = new();

    [ForeignKey("Manager")] public Guid? ManagerId { get; set; }

    public virtual Manager? Manager { get; set; } = new();

    [MaxLength(30)]
    public string? Description { get; set; }

    // Rename to category
    // It's a unique definition that redefine the behaviour of the transaction
    [ForeignKey("Tag")] public Guid? TagId { get; set; }

    public virtual Tag Tag { get; set; } = new();
    
    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
}