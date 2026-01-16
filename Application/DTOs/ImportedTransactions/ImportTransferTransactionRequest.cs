using SFManagement.Application.DTOs.Common;
﻿namespace SFManagement.Application.DTOs.ImportedTransactions;

public class ImportTransferTransactionRequest
{
    public required IFormFile File { get; set; }

    public Guid WalletId { get; set; }
}