using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Exceptions;
using SFManagement.Models.Entities;
using SFManagement.Models.Transactions;
using SFManagement.Enums.ImportedFiles;

namespace SFManagement.Services;

public class ImportedTransactionService : BaseService<ImportedTransaction>
{
    public ImportedTransactionService(DataContext context, IHttpContextAccessor httpContextAccessor) 
        : base(context, httpContextAccessor)
    {
    }

    /// <summary>
    /// Expose the DataContext for controller access
    /// </summary>
    public DataContext Context => context;

    /// <summary>
    /// Imports transactions from an OFX file
    /// Replaces the legacy OfxService.Add method
    /// </summary>
    public async Task<List<ImportedTransaction>> ImportOfxFile(IFormFile file, Guid baseAssetHolderId)
    {
        // Validate BaseAssetHolder exists and is a Bank
        var baseAssetHolder = await context.BaseAssetHolders
            .Include(bah => bah.Bank)
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (baseAssetHolder?.Bank == null)
            throw new BusinessException("BaseAssetHolder not found or is not a Bank", "INVALID_ASSET_HOLDER");

        var fileHash = await CalculateFileHash(file);
        var transactions = new List<ImportedTransaction>();
        
        // Parse OFX content using legacy logic
        var ofxTransactions = await ParseOfxContent(file);
        
        foreach (var ofxTx in ofxTransactions)
        {
            // Check for duplicates using ExternalReferenceId (FitId)
            var existingTransaction = await context.ImportedTransactions
                .FirstOrDefaultAsync(it => it.ExternalReferenceId == ofxTx.FitId && 
                                          it.BaseAssetHolderId == baseAssetHolderId &&
                                          it.FileType == ImportFileType.Ofx &&
                                          !it.DeletedAt.HasValue);
            
            if (existingTransaction != null)
            {
                // Mark as duplicate and skip
                continue;
            }
            
            var importedTransaction = new ImportedTransaction
            {
                Date = ofxTx.Date,
                Amount = ofxTx.Value,
                Description = ofxTx.Description,
                ExternalReferenceId = ofxTx.FitId,
                BaseAssetHolderId = baseAssetHolderId,
                FileType = ImportFileType.Ofx,
                FileName = file.FileName,
                FileHash = fileHash,
                FileSizeBytes = file.Length,
                FileMetadata = JsonSerializer.Serialize(new
                {
                    FitId = ofxTx.FitId,
                    OriginalAmount = ofxTx.Value,
                    BankId = baseAssetHolderId
                }),
                Status = ImportedTransactionStatus.Processed,
                ProcessedAt = DateTime.UtcNow
            };
            
            transactions.Add(importedTransaction);
        }
        
        if (transactions.Count == 0)
        {
            throw new BusinessException("No new transactions found in OFX file", "NO_NEW_TRANSACTIONS");
        }
        
        // Add all transactions
        foreach (var transaction in transactions)
        {
            await Add(transaction);
        }
        
        return transactions;
    }

    /// <summary>
    /// Imports transactions from an Excel file
    /// Replaces the legacy ExcelService import methods
    /// </summary>
    public async Task<List<ImportedTransaction>> ImportExcelFile(
        IFormFile file, 
        Guid baseAssetHolderId, 
        ExcelImportType importType,
        List<(int Column, string Name)> columnMapping)
    {
        // Validate BaseAssetHolder exists and is a PokerManager
        var baseAssetHolder = await context.BaseAssetHolders
            .Include(bah => bah.PokerManager)
            .FirstOrDefaultAsync(bah => bah.Id == baseAssetHolderId && !bah.DeletedAt.HasValue);
        
        if (baseAssetHolder?.PokerManager == null)
            throw new BusinessException("BaseAssetHolder not found or is not a PokerManager", "INVALID_ASSET_HOLDER");

        var fileHash = await CalculateFileHash(file);
        var transactions = new List<ImportedTransaction>();
        
        // Parse Excel content using legacy logic
        var excelRows = ReadExcelFile(file, columnMapping);
        
        foreach (var row in excelRows)
        {
            try
            {
                var importedTransaction = await ProcessExcelRow(
                    row, 
                    baseAssetHolderId, 
                    importType, 
                    file.FileName, 
                    fileHash, 
                    file.Length);
                
                if (importedTransaction != null)
                {
                    transactions.Add(importedTransaction);
                }
            }
            catch (Exception ex)
            {
                // Create a failed transaction record for tracking
                var failedTransaction = new ImportedTransaction
                {
                    Date = DateTime.UtcNow,
                    Amount = 0,
                    Description = "Failed to process Excel row",
                    BaseAssetHolderId = baseAssetHolderId,
                    FileType = ImportFileType.Excel,
                    FileName = file.FileName,
                    FileHash = fileHash,
                    FileSizeBytes = file.Length,
                    Status = ImportedTransactionStatus.Failed,
                    ProcessingError = ex.Message
                };
                
                transactions.Add(failedTransaction);
            }
        }
        
        if (transactions.Count == 0)
        {
            throw new BusinessException("No transactions could be processed from Excel file", "NO_PROCESSABLE_TRANSACTIONS");
        }
        
        // Add all transactions
        foreach (var transaction in transactions)
        {
            await Add(transaction);
        }
        
        return transactions;
    }

    /// <summary>
    /// Reconciles an imported transaction with a BaseTransaction
    /// Replaces the legacy reconciliation logic
    /// </summary>
    public async Task<ImportedTransaction> ReconcileTransaction(
        Guid importedTransactionId, 
        Guid baseTransactionId, 
        ReconciledTransactionType transactionType,
        string? notes = null)
    {
        var importedTransaction = await Get(importedTransactionId);
        if (importedTransaction == null)
            throw new EntityNotFoundException("ImportedTransaction", importedTransactionId);

        // Validate the BaseTransaction exists based on type
        var baseTransactionExists = transactionType switch
        {
            ReconciledTransactionType.Fiat => await context.FiatAssetTransactions
                .AnyAsync(ft => ft.Id == baseTransactionId && !ft.DeletedAt.HasValue),
            ReconciledTransactionType.Digital => await context.DigitalAssetTransactions
                .AnyAsync(dt => dt.Id == baseTransactionId && !dt.DeletedAt.HasValue),
            ReconciledTransactionType.Settlement => await context.SettlementTransactions
                .AnyAsync(st => st.Id == baseTransactionId && !st.DeletedAt.HasValue),
            _ => throw new BusinessException($"Invalid transaction type: {transactionType}", "INVALID_TRANSACTION_TYPE")
        };
        
        if (!baseTransactionExists)
            throw new EntityNotFoundException($"{transactionType}Transaction", baseTransactionId);

        // Check if already reconciled
        if (importedTransaction.IsReconciled)
            throw new BusinessException("ImportedTransaction is already reconciled", "ALREADY_RECONCILED");

        // Perform reconciliation
        importedTransaction.ReconciledTransactionType = transactionType;
        importedTransaction.ReconciledTransactionId = baseTransactionId;
        importedTransaction.ReconciledAt = DateTime.UtcNow;
        importedTransaction.ReconciliationNotes = notes;
        importedTransaction.Status = ImportedTransactionStatus.Reconciled;
        importedTransaction.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync();
        
        return importedTransaction;
    }

    /// <summary>
    /// Gets all imported transactions for a specific file
    /// </summary>
    public async Task<List<ImportedTransaction>> GetTransactionsByFile(string fileName, Guid baseAssetHolderId)
    {
        return await context.ImportedTransactions
            .Include(it => it.BaseAssetHolder)
            .Where(it => it.FileName == fileName && 
                        it.BaseAssetHolderId == baseAssetHolderId && 
                        !it.DeletedAt.HasValue)
            .OrderBy(it => it.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Gets unreconciled imported transactions for reconciliation
    /// </summary>
    public async Task<List<ImportedTransaction>> GetUnreconciledTransactions(Guid baseAssetHolderId)
    {
        return await context.ImportedTransactions
            .Include(it => it.BaseAssetHolder)
            .Where(it => it.BaseAssetHolderId == baseAssetHolderId && 
                        !it.ReconciledTransactionId.HasValue &&
                        it.Status != ImportedTransactionStatus.Failed &&
                        it.Status != ImportedTransactionStatus.Duplicate &&
                        !it.DeletedAt.HasValue)
            .OrderBy(it => it.Date)
            .ToListAsync();
    }

    /// <summary>
    /// Finds potential matches for reconciliation based on amount and date
    /// </summary>
    public async Task<List<(ReconciledTransactionType Type, Guid Id, DateTime Date, decimal Amount, string? Description)>> FindPotentialMatches(
        Guid importedTransactionId, 
        int daysTolerance = 3,
        decimal amountTolerance = 0.01m)
    {
        var importedTransaction = await Get(importedTransactionId);
        if (importedTransaction == null)
            throw new EntityNotFoundException("ImportedTransaction", importedTransactionId);

        var startDate = importedTransaction.Date.AddDays(-daysTolerance);
        var endDate = importedTransaction.Date.AddDays(daysTolerance);
        var minAmount = importedTransaction.Amount - amountTolerance;
        var maxAmount = importedTransaction.Amount + amountTolerance;

        var results = new List<(ReconciledTransactionType Type, Guid Id, DateTime Date, decimal Amount, string? Description)>();

        // Search Fiat transactions
        var fiatTransactions = await context.FiatAssetTransactions
            .Include(ft => ft.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(ft => ft.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Where(ft => ft.Date >= startDate && ft.Date <= endDate &&
                        ft.AssetAmount >= minAmount && ft.AssetAmount <= maxAmount &&
                        !ft.DeletedAt.HasValue &&
                        (ft.SenderWalletIdentifier.AssetPool.BaseAssetHolderId == importedTransaction.BaseAssetHolderId ||
                         ft.ReceiverWalletIdentifier.AssetPool.BaseAssetHolderId == importedTransaction.BaseAssetHolderId))
            .Select(ft => new { ft.Id, ft.Date, ft.AssetAmount, ft.Description })
            .ToListAsync();

        results.AddRange(fiatTransactions.Select(ft => (ReconciledTransactionType.Fiat, ft.Id, ft.Date, ft.AssetAmount, ft.Description)));

        // Search Digital transactions
        var digitalTransactions = await context.DigitalAssetTransactions
            .Include(dt => dt.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(dt => dt.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Where(dt => dt.Date >= startDate && dt.Date <= endDate &&
                        dt.AssetAmount >= minAmount && dt.AssetAmount <= maxAmount &&
                        !dt.DeletedAt.HasValue &&
                        (dt.SenderWalletIdentifier.AssetPool.BaseAssetHolderId == importedTransaction.BaseAssetHolderId ||
                         dt.ReceiverWalletIdentifier.AssetPool.BaseAssetHolderId == importedTransaction.BaseAssetHolderId))
            .Select(dt => new { dt.Id, dt.Date, dt.AssetAmount, dt.Description })
            .ToListAsync();

        results.AddRange(digitalTransactions.Select(dt => (ReconciledTransactionType.Digital, dt.Id, dt.Date, dt.AssetAmount, dt.Description)));

        // Search Settlement transactions
        var settlementTransactions = await context.SettlementTransactions
            .Include(st => st.SenderWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Include(st => st.ReceiverWalletIdentifier)
                .ThenInclude(wi => wi.AssetPool)
            .Where(st => st.Date >= startDate && st.Date <= endDate &&
                        st.AssetAmount >= minAmount && st.AssetAmount <= maxAmount &&
                        !st.DeletedAt.HasValue &&
                        (st.SenderWalletIdentifier.AssetPool.BaseAssetHolderId == importedTransaction.BaseAssetHolderId ||
                         st.ReceiverWalletIdentifier.AssetPool.BaseAssetHolderId == importedTransaction.BaseAssetHolderId))
            .Select(st => new { st.Id, st.Date, st.AssetAmount, st.Description })
            .ToListAsync();

        results.AddRange(settlementTransactions.Select(st => (ReconciledTransactionType.Settlement, st.Id, st.Date, st.AssetAmount, st.Description)));

        // Sort by date and amount difference
        return results
            .OrderBy(r => Math.Abs((r.Date - importedTransaction.Date).TotalDays))
            .ThenBy(r => Math.Abs(r.Amount - importedTransaction.Amount))
            .ToList();
    }

    #region Private Helper Methods

    /// <summary>
    /// Calculates SHA256 hash of the uploaded file
    /// </summary>
    private static async Task<string> CalculateFileHash(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var sha256 = SHA256.Create();
        var hashBytes = await Task.Run(() => sha256.ComputeHash(stream));
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Parses OFX file content (adapted from legacy OfxService)
    /// </summary>
    private static async Task<List<OfxTransactionData>> ParseOfxContent(IFormFile file)
    {
        using var stream = new StreamReader(file.OpenReadStream());
        var lines = new List<string>();

        string? line;
        while ((line = await stream.ReadLineAsync()) != null)
        {
            lines.Add(line);
        }

        var tags = lines.Where(x =>
            x.Contains("<STMTTRN>") || x.Contains("<TRNTYPE>") || x.Contains("<DTPOSTED>") ||
            x.Contains("<TRNAMT>") || x.Contains("<FITID>") || x.Contains("<CHECKNUM>") || x.Contains("<MEMO>"));

        var rootElement = new XElement("root");
        XElement? currentTransaction = null;

        foreach (var tagLine in tags)
        {
            if (tagLine.Contains("<STMTTRN>"))
            {
                currentTransaction = new XElement("STMTTRN");
                rootElement.Add(currentTransaction);
                continue;
            }

            if (currentTransaction != null)
            {
                var tagName = GetTagName(tagLine);
                var tagValue = GetTagValue(tagLine);
                var element = new XElement(tagName, tagValue);
                currentTransaction.Add(element);
            }
        }

        var transactions = new List<OfxTransactionData>();
        foreach (var element in rootElement.Descendants("STMTTRN"))
        {
            transactions.Add(ParseOfxTransaction(element));
        }

        return transactions;
    }

    /// <summary>
    /// Parses individual OFX transaction element
    /// </summary>
    private static OfxTransactionData ParseOfxTransaction(XElement element)
    {
        var transaction = new OfxTransactionData();

        // Parse date
        var dtposted = element.Element("DTPOSTED")?.Value;
        if (!string.IsNullOrEmpty(dtposted) && 
            DateTime.TryParseExact(dtposted, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            transaction.Date = date;
        }

        // Parse amount
        var trnamt = element.Element("TRNAMT")?.Value;
        if (!string.IsNullOrEmpty(trnamt) && 
            decimal.TryParse(trnamt, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowLeadingSign, 
                CultureInfo.InvariantCulture, out var value))
        {
            transaction.Value = Math.Abs(value); // Always store positive amount
        }

        transaction.Description = element.Element("MEMO")?.Value;
        transaction.FitId = element.Element("FITID")?.Value ?? "not_found";

        return transaction;
    }

    /// <summary>
    /// Reads Excel file content (adapted from legacy ExcelService)
    /// </summary>
    private static List<List<(string Value, string Name)>> ReadExcelFile(IFormFile file, List<(int Column, string Name)> fields)
    {
        var rows = new List<List<(string Value, string Name)>>();

        using var stream = file.OpenReadStream();
        using var package = new ExcelPackage(stream);
        
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        for (var row = 1; row <= rowCount; row++)
        {
            var rowValues = new List<(string Value, string Name)>();

            foreach (var field in fields)
            {
                var fieldValue = worksheet.Cells[row, field.Column].Text;
                rowValues.Add((fieldValue, field.Name));
            }

            rows.Add(rowValues);
        }

        return rows;
    }

    /// <summary>
    /// Processes individual Excel row into ImportedTransaction
    /// </summary>
    private async Task<ImportedTransaction?> ProcessExcelRow(
        List<(string Value, string Name)> row,
        Guid baseAssetHolderId,
        ExcelImportType importType,
        string fileName,
        string fileHash,
        long fileSize)
    {
        try
        {
            var amount = decimal.Parse(
                row.FirstOrDefault(x => x.Name == "Value" || x.Name == "Amount").Value, 
                new CultureInfo("pt-BR"));

            var dateStr = row.FirstOrDefault(x => x.Name == "CreatedAt" || x.Name == "Date").Value;
            var formats = new[] { "d/M/yyyy H:mm", "d/M/yy H:mm", "d/M/yy HH:mm", "d/M/yyyy HH:mm" };
            
            if (!DateTime.TryParseExact(dateStr, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
            {
                throw new FormatException($"Unable to parse date: {dateStr}");
            }

            var description = row.FirstOrDefault(x => x.Name == "Description").Value;
            var walletIdentifier = row.FirstOrDefault(x => x.Name == "WalletIdentifier").Value;
            var assetPool = row.FirstOrDefault(x => x.Name == "AssetPool").Value;

            // Generate external reference ID for duplicate detection
            var externalRefId = $"{date:yyyyMMdd}_{amount}_{walletIdentifier}_{assetPool}";

            // Check for duplicates
            var existingTransaction = await context.ImportedTransactions
                .FirstOrDefaultAsync(it => it.ExternalReferenceId == externalRefId && 
                                          it.BaseAssetHolderId == baseAssetHolderId &&
                                          !it.DeletedAt.HasValue);

            if (existingTransaction != null)
            {
                return null; // Skip duplicate
            }

            var metadata = JsonSerializer.Serialize(new
            {
                ImportType = importType.ToString(),
                WalletIdentifier = walletIdentifier,
                AssetPool = assetPool,
                From = row.FirstOrDefault(x => x.Name == "From").Value,
                To = row.FirstOrDefault(x => x.Name == "To").Value
            });

            return new ImportedTransaction
            {
                Date = date,
                Amount = Math.Abs(amount),
                Description = description,
                ExternalReferenceId = externalRefId,
                BaseAssetHolderId = baseAssetHolderId,
                FileType = ImportFileType.Excel,
                FileName = fileName,
                FileHash = fileHash,
                FileSizeBytes = fileSize,
                FileMetadata = metadata,
                Status = ImportedTransactionStatus.Processed,
                ProcessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            throw new BusinessException($"Failed to process Excel row: {ex.Message}", ex, "EXCEL_ROW_PROCESSING_FAILED");
        }
    }

    private static string GetTagName(string line)
    {
        var posInit = line.IndexOf("<") + 1;
        var posEnd = line.IndexOf(">");
        posEnd = posEnd - posInit;
        return line.Substring(posInit, posEnd);
    }

    private static string GetTagValue(string line)
    {
        var posInit = line.IndexOf(">") + 1;
        var retValue = line.Substring(posInit).Trim();
        if (retValue.Contains("["))
            retValue = retValue.Substring(0, 8);
        return retValue;
    }

    #endregion
}

/// <summary>
/// Data structure for OFX transaction parsing
/// </summary>
public class OfxTransactionData
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public string? Description { get; set; }
    public string FitId { get; set; } = string.Empty;
} 