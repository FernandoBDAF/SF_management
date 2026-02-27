using SFManagement.Infrastructure.Logging;
using Microsoft.AspNetCore.Authorization;

namespace SFManagement.Infrastructure.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAttribute : AuthorizeAttribute
{
    public RequireRoleAttribute(string role) : base($"Role:{role}")
    {
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(string permission) : base($"Permission:{permission}")
    {
    }
}

public static class Auth0Roles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string User = "user";
    public const string Viewer = "viewer";
}

public static class Auth0Permissions
{
    // User management
    public const string ReadUsers = "read:users";
    public const string CreateUsers = "create:users";
    public const string UpdateUsers = "update:users";
    public const string DeleteUsers = "delete:users";
    
    // Client management
    public const string ReadClients = "read:clients";
    public const string CreateClients = "create:clients";
    public const string UpdateClients = "update:clients";
    public const string DeleteClients = "delete:clients";

    // Member management
    public const string ReadMembers = "read:members";
    public const string CreateMembers = "create:members";
    public const string UpdateMembers = "update:members";
    public const string DeleteMembers = "delete:members";

    // Bank management
    public const string ReadBanks = "read:banks";
    public const string CreateBanks = "create:banks";
    public const string UpdateBanks = "update:banks";
    public const string DeleteBanks = "delete:banks";

    // Poker manager management
    public const string ReadManagers = "read:managers";
    public const string CreateManagers = "create:managers";
    public const string UpdateManagers = "update:managers";
    public const string DeleteManagers = "delete:managers";
    
    // Transaction management
    public const string ReadTransactions = "read:transactions";
    public const string CreateTransactions = "create:transactions";
    public const string UpdateTransactions = "update:transactions";
    public const string DeleteTransactions = "delete:transactions";
    
    // Financial data
    public const string ReadFinancialData = "read:financial_data";
    public const string CreateFinancialData = "create:financial_data";
    public const string UpdateFinancialData = "update:financial_data";
    public const string DeleteFinancialData = "delete:financial_data";

    // Import management
    public const string ReadImports = "read:imports";
    public const string CreateImports = "create:imports";
    public const string DeleteImports = "delete:imports";

    // Category management
    public const string ReadCategories = "read:categories";
    public const string CreateCategories = "create:categories";
    public const string UpdateCategories = "update:categories";
    public const string DeleteCategories = "delete:categories";

    // Wallet management
    public const string ReadWallets = "read:wallets";
    public const string CreateWallets = "create:wallets";
    public const string UpdateWallets = "update:wallets";
    public const string DeleteWallets = "delete:wallets";

    // Settlement management
    public const string ReadSettlements = "read:settlements";
    public const string CreateSettlements = "create:settlements";

    // Diagnostics
    public const string ReadDiagnostics = "read:diagnostics";

    // Ledger
    public const string ReadLedger = "read:ledger";

    // Planned finance modules
    public const string ReadInvoices = "read:invoices";
    public const string CreateInvoices = "create:invoices";
    public const string UpdateInvoices = "update:invoices";
    public const string DeleteInvoices = "delete:invoices";

    public const string ReadExpenses = "read:expenses";
    public const string CreateExpenses = "create:expenses";
    public const string UpdateExpenses = "update:expenses";
    public const string DeleteExpenses = "delete:expenses";
} 