namespace SFManagement.Enums;

// Poker wallet specific metadata fields  
public enum PokerWalletMetadata
{
    SiteName,          // Poker site name (PokerStars, GGPoker, etc.)
    PlayerNickname,    // Site-specific player nickname
    PlayerEmail,       // Player email
    VIPLevel,          // VIP status (Bronze, Silver, Gold, etc.)
    AccountStatus,     // Account status (Verified, Pending, etc.)
    InputForTransactions // How to fill the input for transactions
} 