using SFManagement.Domain.Common;
﻿using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SFManagement.Domain.Entities.Support;
// These tags influence how the system processes or interprets the entity’s financial actions.
public class Category : BaseDomain
{
    [MaxLength(64)]
    public string Description { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public virtual Category? Parent { get; set; }

    public virtual List<Category> Children { get; set; } = new();
}