using SFManagement.Application.DTOs.Common;
﻿namespace SFManagement.Application.DTOs.ImportedTransactions;

public class ImportTransferTransactionRequest
{
    public IFormFile File { get; set; }

    public Guid WalletId { get; set; }
}