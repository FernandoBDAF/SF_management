using SFManagement.Application.DTOs.Common;
﻿namespace SFManagement.Application.DTOs.Support;

public class CategoryRequest
{
    public string? Description { get; set; }

    public Guid? CategoryId { get; set; }
}