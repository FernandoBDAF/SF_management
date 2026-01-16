namespace SFManagement.Domain.Enums.Metadata;

// Crypto wallet specific metadata fields
public enum CryptoWalletMetadata
{
    WalletAddress,     // Blockchain wallet address
    ExchangeName,      // Exchange name (if applicable)
    WalletCategory,    // Hot, Cold, Exchange, etc.
    NetworkType,       // Mainnet, Testnet, etc.
    ApiKey,            // Exchange API key (encrypted)
    ApiSecret,         // Exchange API secret (encrypted)
    DisplayName        // User-friendly display name
} 