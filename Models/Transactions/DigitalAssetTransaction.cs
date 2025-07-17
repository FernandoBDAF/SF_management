using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Enums.AssetInfrastructure;

namespace SFManagement.Models.Transactions;

public class DigitalAssetTransaction : BaseTransaction
{
    public AssetType? BalanceAs { get; set; }
    
    [Precision(18, 4)] public decimal? ConversionRate { get; set; }
    
    [Precision(18, 4)] public decimal? Rate { get; set; }
}