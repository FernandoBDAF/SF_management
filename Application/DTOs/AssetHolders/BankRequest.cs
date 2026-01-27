using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Support;
﻿using System.ComponentModel.DataAnnotations;

namespace SFManagement.Application.DTOs.AssetHolders;

public class BankRequest : BaseAssetHolderRequest
{
    [Required]
    [StringLength(10, MinimumLength = 1)]
    public string Code { get; set; } = string.Empty;
}