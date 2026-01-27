using SFManagement.Domain.Common;
﻿using Microsoft.EntityFrameworkCore;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Domain.Entities.Transactions;

public class DigitalAssetTransaction : BaseTransaction
{
    public AssetType? BalanceAs { get; set; }
    
    [Precision(18, 4)] public decimal? ConversionRate { get; set; }
    
    [Precision(18, 4)] public decimal? Rate { get; set; }
}