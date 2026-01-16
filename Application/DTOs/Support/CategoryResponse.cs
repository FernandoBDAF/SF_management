using SFManagement.Application.DTOs.Common;
﻿namespace SFManagement.Application.DTOs.Support;

public class CategoryResponse : BaseResponse
{
    public string? Description { get; set; }

    public Guid? ParentId { get; set; }

    public List<CategoryResponse>? Children { get; set; }
}