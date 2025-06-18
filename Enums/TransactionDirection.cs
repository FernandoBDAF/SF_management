namespace SFManagement.Enums;

// The reference is the AssetHolder - if its sending an asset it's an
// expense or an income otherwise.
public enum TransactionDirection
{
    Income = 1,
    Expense = 2
}