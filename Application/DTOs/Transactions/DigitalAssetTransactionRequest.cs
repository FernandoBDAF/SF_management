using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
﻿using SFManagement.Domain.Enums;
using SFManagement.Domain.Enums.Assets;

namespace SFManagement.Application.DTOs.Transactions;

public class DigitalAssetTransactionRequest : BaseTransactionRequest
{
    public AssetType? BalanceAs { get; set; }
    
    public decimal? ConversionRate { get; set; }
    
    public decimal? Rate { get; set; }
    
    public Guid? ExcelId { get; set; }
}