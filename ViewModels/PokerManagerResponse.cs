using SFManagement.Models.Entities;

namespace SFManagement.ViewModels;

public class PokerManagerResponse : BaseAssetHolderResponse
{
    // Remove redundant collections - these should be accessed through separate endpoints
    // Excels, InitialBalances, ContactPhones, AssetWallets, WalletIdentifiers create circular references
    // and performance issues in responses. Use dedicated endpoints instead.
}