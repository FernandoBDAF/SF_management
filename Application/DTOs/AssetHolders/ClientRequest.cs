using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Support;
﻿using System.ComponentModel.DataAnnotations;

namespace SFManagement.Application.DTOs.AssetHolders;

public class ClientRequest : BaseAssetHolderRequest
{
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime? Birthday { get; set; }
    
    /// <summary>
    /// Validates that the birthday is not in the future
    /// </summary>
    public bool IsValidBirthday => !Birthday.HasValue || Birthday.Value <= DateTime.Now;
}