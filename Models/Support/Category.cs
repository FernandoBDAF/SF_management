using SFManagement.Models.Transactions;
using SFManagement.Enums;

namespace SFManagement.Models.Support;
// These tags influence how the system processes or interprets the entity’s financial actions.
public class Category : BaseDomain
{
    public string Description { get; set; }

    public Guid? CategoryId { get; set; }
    public virtual Category? Parent { get; set; }

    public virtual List<Category> Children { get; set; } = new();
}