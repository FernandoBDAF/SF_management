using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SFManagement.Enums;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ImportedTransactionController : BaseApiController<ImportedTransaction, ImportTransactionRequest, ImportedTransactionResponse>
{
    private readonly ImportedTransactionService _importedTransactionService;
    private readonly IMapper _localMapper;

    public ImportedTransactionController(
        BaseService<ImportedTransaction> service, 
        IMapper mapper,
        ImportedTransactionService importedTransactionService) 
        : base(service, mapper)
    {
        _importedTransactionService = importedTransactionService;
        _localMapper = mapper;
    }

    /// <summary>
    /// Imports transactions from an OFX file (Bank statements)
    /// Replaces the legacy OfxController.Post method
    /// </summary>
    [HttpPost("import/ofx")]
    public async Task<ActionResult<ImportSummaryResponse>> ImportOfxFile([FromForm] ImportOfxRequest request)
    {
        try
        {
            var transactions = await _importedTransactionService.ImportOfxFile(
                request.File, 
                request.BaseAssetHolderId);

            var response = new ImportSummaryResponse
            {
                FileName = request.File.FileName,
                FileType = ImportFileType.Ofx,
                FileTypeName = "OFX File",
                BaseAssetHolderId = request.BaseAssetHolderId,
                TotalTransactions = transactions.Count,
                ProcessedTransactions = transactions.Count(t => t.Status == ImportedTransactionStatus.Processed),
                FailedTransactions = transactions.Count(t => t.Status == ImportedTransactionStatus.Failed),
                DuplicateTransactions = 0, // Duplicates are filtered out during import
                ReconciledTransactions = 0, // None are reconciled immediately
                ImportedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetTransactionsByFile), 
                new { fileName = request.File.FileName, baseAssetHolderId = request.BaseAssetHolderId }, 
                response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Imports buy transactions from Excel file (Poker transactions)
    /// Replaces the legacy ExcelController.ImportBuyTransactions method
    /// </summary>
    [HttpPost("import/excel/buy")]
    public async Task<ActionResult<ImportSummaryResponse>> ImportExcelBuyTransactions([FromForm] ImportExcelRequest request)
    {
        request.ImportType = ExcelImportType.BuyTransactions;
        
        // Default column mapping for buy transactions
        request.ColumnMapping ??= new List<ColumnMapping>
        {
            new() { Column = 1, Name = "WalletIdentifier" },
            new() { Column = 2, Name = "Value" },
            new() { Column = 3, Name = "AssetPool" },
            new() { Column = 6, Name = "CreatedAt" },
            new() { Column = 8, Name = "Description" }
        };

        return await ImportExcelFile(request);
    }

    /// <summary>
    /// Imports sell transactions from Excel file (Poker transactions)
    /// Replaces the legacy ExcelController.ImportSellTransactions method
    /// </summary>
    [HttpPost("import/excel/sell")]
    public async Task<ActionResult<ImportSummaryResponse>> ImportExcelSellTransactions([FromForm] ImportExcelRequest request)
    {
        request.ImportType = ExcelImportType.SellTransactions;
        
        // Default column mapping for sell transactions
        request.ColumnMapping ??= new List<ColumnMapping>
        {
            new() { Column = 1, Name = "WalletIdentifier" },
            new() { Column = 2, Name = "Value" },
            new() { Column = 3, Name = "AssetPool" },
            new() { Column = 6, Name = "CreatedAt" },
            new() { Column = 8, Name = "Description" }
        };

        return await ImportExcelFile(request);
    }

    /// <summary>
    /// Imports transfer transactions from Excel file (Poker transactions)
    /// Replaces the legacy ExcelController.ImportTransferTransactions method
    /// </summary>
    [HttpPost("import/excel/transfer")]
    public async Task<ActionResult<ImportSummaryResponse>> ImportExcelTransferTransactions([FromForm] ImportExcelRequest request)
    {
        request.ImportType = ExcelImportType.TransferTransactions;
        
        // Default column mapping for transfer transactions
        request.ColumnMapping ??= new List<ColumnMapping>
        {
            new() { Column = 1, Name = "From" },
            new() { Column = 2, Name = "To" },
            new() { Column = 3, Name = "CreatedAt" },
            new() { Column = 4, Name = "Value" },
            new() { Column = 5, Name = "Description" }
        };

        return await ImportExcelFile(request);
    }

    /// <summary>
    /// Generic Excel import method
    /// </summary>
    [HttpPost("import/excel")]
    public async Task<ActionResult<ImportSummaryResponse>> ImportExcelFile([FromForm] ImportExcelRequest request)
    {
        try
        {
            var columnMapping = request.ColumnMapping?.Select(cm => (cm.Column, cm.Name)).ToList() 
                ?? new List<(int, string)>();

            var transactions = await _importedTransactionService.ImportExcelFile(
                request.File, 
                request.BaseAssetHolderId, 
                request.ImportType,
                columnMapping);

            var response = new ImportSummaryResponse
            {
                FileName = request.File.FileName,
                FileType = ImportFileType.Excel,
                FileTypeName = "Excel File",
                BaseAssetHolderId = request.BaseAssetHolderId,
                TotalTransactions = transactions.Count,
                ProcessedTransactions = transactions.Count(t => t.Status == ImportedTransactionStatus.Processed),
                FailedTransactions = transactions.Count(t => t.Status == ImportedTransactionStatus.Failed),
                DuplicateTransactions = 0, // Duplicates are filtered out during import
                ReconciledTransactions = 0, // None are reconciled immediately
                Errors = transactions.Where(t => t.HasErrors).Select(t => t.ProcessingError ?? "Unknown error").ToList(),
                ImportedAt = DateTime.UtcNow
            };

            return CreatedAtAction(nameof(GetTransactionsByFile), 
                new { fileName = request.File.FileName, baseAssetHolderId = request.BaseAssetHolderId }, 
                response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Reconciles an imported transaction with a BaseTransaction
    /// Replaces the legacy ExcelController.Reconciliation method
    /// </summary>
    [HttpPost("reconcile")]
    public async Task<ActionResult<ReconciliationResponse>> ReconcileTransaction([FromBody] ReconcileTransactionRequest request)
    {
        try
        {
            var reconciledTransaction = await _importedTransactionService.ReconcileTransaction(
                request.ImportedTransactionId,
                request.BaseTransactionId,
                request.Notes);

            var response = new ReconciliationResponse
            {
                ImportedTransactionId = request.ImportedTransactionId,
                BaseTransactionId = request.BaseTransactionId,
                ReconciledAt = reconciledTransaction.ReconciledAt!.Value,
                Notes = request.Notes,
                ImportedTransaction = _localMapper.Map<ImportedTransactionResponse>(reconciledTransaction)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets all imported transactions for a specific file
    /// </summary>
    [HttpGet("file/{fileName}/asset-holder/{baseAssetHolderId:guid}")]
    public async Task<ActionResult<FileTransactionsResponse>> GetTransactionsByFile(string fileName, Guid baseAssetHolderId)
    {
        try
        {
            var transactions = await _importedTransactionService.GetTransactionsByFile(fileName, baseAssetHolderId);
            
            if (!transactions.Any())
                return NotFound($"No transactions found for file '{fileName}'");

            var response = new FileTransactionsResponse
            {
                FileName = fileName,
                FileType = transactions.First().FileType,
                FileTypeName = transactions.First().FileType.ToString(),
                FileHash = transactions.First().FileHash,
                FileSizeBytes = transactions.First().FileSizeBytes,
                BaseAssetHolderId = baseAssetHolderId,
                Transactions = _localMapper.Map<List<ImportedTransactionResponse>>(transactions),
                TotalTransactions = transactions.Count,
                ReconciledTransactions = transactions.Count(t => t.IsReconciled),
                PendingTransactions = transactions.Count(t => t.Status == ImportedTransactionStatus.Pending),
                FailedTransactions = transactions.Count(t => t.HasErrors),
                FirstTransactionDate = transactions.Min(t => t.Date),
                LastTransactionDate = transactions.Max(t => t.Date),
                TotalAmount = transactions.Sum(t => t.Amount)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets unreconciled imported transactions for an asset holder
    /// </summary>
    [HttpGet("unreconciled/asset-holder/{baseAssetHolderId:guid}")]
    public async Task<ActionResult<List<ImportedTransactionResponse>>> GetUnreconciledTransactions(Guid baseAssetHolderId)
    {
        try
        {
            var transactions = await _importedTransactionService.GetUnreconciledTransactions(baseAssetHolderId);
            var response = _localMapper.Map<List<ImportedTransactionResponse>>(transactions);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Finds potential matches for reconciliation
    /// </summary>
    [HttpPost("find-matches")]
    public async Task<ActionResult<PotentialMatchesResponse>> FindPotentialMatches([FromBody] FindMatchesRequest request)
    {
        try
        {
            var importedTransaction = await _importedTransactionService.Get(request.ImportedTransactionId);
            if (importedTransaction == null)
                return NotFound($"ImportedTransaction {request.ImportedTransactionId} not found");

            var matches = await _importedTransactionService.FindPotentialMatches(
                request.ImportedTransactionId,
                request.DaysTolerance,
                request.AmountTolerance);

            var potentialMatches = matches.Select(bt => new PotentialMatch
            {
                Transaction = new BaseTransactionSummary
                {
                    Id = bt.Id,
                    Date = bt.Date,
                    AssetAmount = bt.AssetAmount,
                    Description = bt.Description,
                    TransactionType = bt.GetType().Name,
                    SenderName = bt.SenderWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown",
                    ReceiverName = bt.ReceiverWalletIdentifier?.AssetPool?.BaseAssetHolder?.Name ?? "Unknown",
                    AssetType = bt.SenderWalletIdentifier?.AssetType ?? AssetType.BrazilianReal
                },
                DaysDifference = Math.Abs((bt.Date - importedTransaction.Date).Days),
                AmountDifference = Math.Abs(bt.AssetAmount - importedTransaction.Amount),
                MatchReasons = GenerateMatchReasons(importedTransaction, bt)
            }).ToList();

            // Calculate match scores
            foreach (var match in potentialMatches)
            {
                match.MatchScore = CalculateMatchScore(match);
            }

            var response = new PotentialMatchesResponse
            {
                ImportedTransaction = _localMapper.Map<ImportedTransactionResponse>(importedTransaction),
                Matches = potentialMatches.OrderByDescending(m => m.MatchScore).ToList(),
                TotalMatches = potentialMatches.Count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Gets dashboard summary for imported transactions
    /// </summary>
    [HttpGet("dashboard/asset-holder/{baseAssetHolderId:guid}")]
    public async Task<ActionResult<ImportedTransactionDashboard>> GetDashboard(Guid baseAssetHolderId)
    {
        try
        {
            var allTransactions = await _importedTransactionService.Context.ImportedTransactions
                .Where(it => it.BaseAssetHolderId == baseAssetHolderId && !it.DeletedAt.HasValue)
                .ToListAsync();

            var dashboard = new ImportedTransactionDashboard
            {
                BaseAssetHolderId = baseAssetHolderId,
                TotalImportedTransactions = allTransactions.Count,
                PendingReconciliation = allTransactions.Count(t => !t.IsReconciled && t.Status == ImportedTransactionStatus.Processed),
                ReconciledTransactions = allTransactions.Count(t => t.IsReconciled),
                FailedTransactions = allTransactions.Count(t => t.HasErrors),
                FilesImported = allTransactions.Select(t => t.FileName).Distinct().Count(),
                LastImportDate = allTransactions.Any() ? allTransactions.Max(t => t.CreatedAt) : null,
                FileTypeSummaries = allTransactions
                    .GroupBy(t => t.FileType)
                    .Select(g => new FileTypeSummary
                    {
                        FileType = g.Key,
                        FileTypeName = g.Key.ToString(),
                        TransactionCount = g.Count(),
                        FileCount = g.Select(t => t.FileName).Distinct().Count(),
                        TotalAmount = g.Sum(t => t.Amount),
                        LastImportDate = g.Max(t => (DateTime?)t.CreatedAt)
                    }).ToList(),
                RecentImports = allTransactions
                    .GroupBy(t => new { t.FileName, t.FileType })
                    .Select(g => new RecentImport
                    {
                        FileName = g.Key.FileName,
                        FileType = g.Key.FileType,
                        TransactionCount = g.Count(),
                        ImportedAt = g.Max(t => t.CreatedAt ?? DateTime.MinValue),
                        Status = g.Any(t => t.HasErrors) ? ImportedTransactionStatus.Failed : 
                                g.All(t => t.IsReconciled) ? ImportedTransactionStatus.Reconciled : 
                                ImportedTransactionStatus.Processed
                    })
                    .OrderByDescending(ri => ri.ImportedAt)
                    .Take(10)
                    .ToList()
            };

            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    #region Private Helper Methods

    private static List<string> GenerateMatchReasons(ImportedTransaction imported, BaseTransaction baseTransaction)
    {
        var reasons = new List<string>();

        if (imported.Date.Date == baseTransaction.Date.Date)
            reasons.Add("Exact date match");
        else if (Math.Abs((imported.Date - baseTransaction.Date).Days) <= 1)
            reasons.Add("Date within 1 day");

        if (imported.Amount == baseTransaction.AssetAmount)
            reasons.Add("Exact amount match");
        else if (Math.Abs(imported.Amount - baseTransaction.AssetAmount) <= 0.01m)
            reasons.Add("Amount within tolerance");

        if (!string.IsNullOrEmpty(imported.Description) && !string.IsNullOrEmpty(baseTransaction.Description))
        {
            if (imported.Description.Contains(baseTransaction.Description) || 
                baseTransaction.Description.Contains(imported.Description))
                reasons.Add("Description similarity");
        }

        return reasons;
    }

    private static double CalculateMatchScore(PotentialMatch match)
    {
        double score = 100.0;

        // Penalize based on date difference
        score -= match.DaysDifference * 10;

        // Penalize based on amount difference
        score -= (double)match.AmountDifference * 5;

        // Bonus for exact matches
        if (match.DaysDifference == 0)
            score += 20;
        if (match.AmountDifference == 0)
            score += 30;

        // Bonus for match reasons
        score += match.MatchReasons.Count * 10;

        return Math.Max(0, score);
    }

    #endregion
} 