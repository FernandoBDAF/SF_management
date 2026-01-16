using SFManagement.Application.DTOs.Common;
﻿namespace SFManagement.Application.DTOs.ImportedTransactions;

public class ImportBuySellTransactionsRequest
{
    public IFormFile File { get; set; }

    public Guid WalletId { get; set; }
}