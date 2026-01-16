using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.Assets;
﻿using SFManagement.Domain.Enums;
using SFManagement.Domain.Entities.Transactions;

namespace SFManagement.Application.DTOs.Transactions;

public class FiatAssetTransactionResponse : BaseTransactionResponse
{
    /// <summary>
    /// Associated OFX transaction information (for bank imports)
    /// </summary>
    public OfxTransactionSummary? OfxTransaction { get; set; }
    
    /// <summary>
    /// Bank-specific information for fiat transactions
    /// </summary>
    public BankTransactionInfo? BankInfo { get; set; }
    
    /// <summary>
    /// PIX transaction details (if applicable)
    /// </summary>
    public PixTransactionInfo? PixInfo { get; set; }
}

/// <summary>
/// OFX transaction summary for fiat transactions
/// </summary>
public class OfxTransactionSummary
{
    public Guid Id { get; set; }
    public string? FitId { get; set; }
    public decimal Value { get; set; }
    public DateTime Date { get; set; }
    public string? Description { get; set; }
    public string? BankName { get; set; }
    public string? FileName { get; set; }
}

/// <summary>
/// Bank-specific transaction information
/// </summary>
public class BankTransactionInfo
{
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? RoutingNumber { get; set; }
    public string? AccountType { get; set; }
}

/// <summary>
/// PIX transaction information
/// </summary>
public class PixTransactionInfo
{
    public string? PixKey { get; set; }
    public string? PixKeyType { get; set; }
    public string? EndToEndId { get; set; }
}