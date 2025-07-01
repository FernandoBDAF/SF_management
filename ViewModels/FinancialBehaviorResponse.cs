namespace SFManagement.ViewModels;

public class FinancialBehaviorResponse : BaseResponse
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public List<FinancialBehaviorResponse>? Children { get; set; }
}