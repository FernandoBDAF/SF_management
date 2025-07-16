using Microsoft.AspNetCore.Authorization;

namespace SFManagement.Authorization;

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
} 