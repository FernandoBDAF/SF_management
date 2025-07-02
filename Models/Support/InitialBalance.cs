using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;

namespace SFManagement.Models.Support;

public class InitialBalance : BaseDomain
{
    [Precision(18, 2)] public decimal Balance { get; set; }
    
    public AssetType BalanceUnit { get; set; }
    
    [Precision(18, 2)] public decimal? ConversionRate { get; set; }
    
    public AssetType? BalanceAs { get; set; }
    
    [Required] public Guid BaseAssetHolderId { get; set; }
}