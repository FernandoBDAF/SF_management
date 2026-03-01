namespace SFManagement.Domain.Enums.Assets;

public enum AssetGroup
{
    // None
    None = 0,

    // Fiat Assets
    FiatAssets = 1,
    
    // Poker Assets  
    PokerAssets = 2,
    
    // Crypto Assets
    CryptoAssets = 3,
    
    // Flexible wallets (system/conversion)
    Flexible = 4,

    // Settlements
    Settlements = 5,
}