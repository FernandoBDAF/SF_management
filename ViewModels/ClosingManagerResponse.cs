using System.ComponentModel.DataAnnotations.Schema;

namespace SFManagement.ViewModels;

public class ClosingManagerResponse : BaseResponse
{
    public Guid ManagerId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public DateTime? DoneAt { get; set; }

    public DateTime? CalculatedAt { get; set; }

    public decimal RakeBruto { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal TotalRakeDiscounts { get; set; }
}