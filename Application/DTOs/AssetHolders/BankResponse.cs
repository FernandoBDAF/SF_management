using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.DTOs.Support;
﻿using SFManagement.Domain.Entities.AssetHolders;

namespace SFManagement.Application.DTOs.AssetHolders;

public class BankResponse : BaseAssetHolderResponse
{
    public string? Code { get; set; }
    
    // Remove redundant collections - these should be accessed through separate endpoints
    // Ofxs collection creates circular references and performance issues in responses
    // Use dedicated endpoints for OFX data instead
}