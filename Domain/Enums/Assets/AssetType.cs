namespace SFManagement.Domain.Enums.Assets;

public enum AssetType
{
    // Miscellaneous
    None = 0,

    // Fiat
    BrazilianReal = 21,
    USDollar = 22,

    // Poker in USDollar
    PokerStars = 101,
    GgPoker = 102,
    YaPoker = 103,
    AmericasCardRoom = 104,
    SupremaPoker = 105,
    AstroPayICash = 106,
    LuxonPoker = 107,

    // Crypto
    Bitcoin = 201,
    Ethereum = 202,
    Litecoin = 203,
    Ripple = 204,
    BitcoinCash = 205,
    Stellar = 206,

    // Internal
}