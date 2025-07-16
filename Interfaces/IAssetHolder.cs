using SFManagement.Models;
using SFManagement.Models.Entities;

namespace SFManagement.Interfaces;

/// <summary>
/// Interface for entities that represent asset holders (Client, Bank, Member, PokerManager)
/// </summary>
public interface IAssetHolder
{
    /// <summary>
    /// The ID of the associated BaseAssetHolder
    /// </summary>
    Guid BaseAssetHolderId { get; set; }
    
    /// <summary>
    /// Navigation property to the BaseAssetHolder
    /// </summary>
    BaseAssetHolder? BaseAssetHolder { get; set; }
}