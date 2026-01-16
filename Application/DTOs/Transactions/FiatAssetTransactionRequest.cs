using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
﻿using SFManagement.Domain.Enums;

namespace SFManagement.Application.DTOs.Transactions;

public class FiatAssetTransactionRequest : BaseTransactionRequest
{
    public Guid? OfxTransactionId { get; set; }
    
    public Guid? BaseAssetHolderId { get; set; }
    
    public Guid? ClientId { get; set; }
    
    public Guid? BankId { get; set; }
}