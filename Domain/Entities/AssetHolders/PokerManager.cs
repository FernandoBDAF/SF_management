using SFManagement.Domain.Common;
﻿using System.ComponentModel.DataAnnotations;
using SFManagement.Domain.Enums;
using SFManagement.Domain.Interfaces;
using SFManagement.Domain.Entities.Transactions;

namespace SFManagement.Domain.Entities.AssetHolders;

public class PokerManager : BaseDomain, IAssetHolder
{
    // public ManagerProfitType ManagerProfitType { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
    public virtual BaseAssetHolder? BaseAssetHolder { get; set; }
    
    public ManagerProfitType? ManagerProfitType { get; set; }
}