using SFManagement.Application.DTOs.Common;
﻿using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.AssetHolders;
using SFManagement.Domain.Entities.Transactions;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Assets;

public class AssetPoolResponse : BaseResponse
{
    public AssetGroup AssetGroup { get; set; }
    
    public decimal? DefaultAgreedCommission { get; set; }
    
    public Guid BaseAssetHolderId { get; set; }
    
    public string? BaseAssetHolderName { get; set; }
    
    public List<WalletIdentifierResponse> WalletIdentifiers { get; set; } = new List<WalletIdentifierResponse>();
}
