using System.Globalization;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using SFManagement.Data;
using SFManagement.Enums;
using SFManagement.Models.Transactions;
using SFManagement.ViewModels;

namespace SFManagement.Services;

public class ExcelService(
    DataContext context,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor,
    DigitalAssetTransactionService digitalAssetTransactionService)
    : BaseService<Excel>(context, httpContextAccessor)
{
    private readonly DigitalAssetTransactionService _digitalAssetTransactionService = digitalAssetTransactionService;

    public override async Task<Excel?> Get(Guid id)
    {
        return await _entity.Include(x => x.ExcelTransactions)
            .FirstOrDefaultAsync(x => x.Id == id && !x.DeletedAt.HasValue);
    }

    public async Task<List<ExcelTransactionResponse>> ImportBuySellTransactions(ExcelRequest request,
        TransactionDirection walletTransactionType)
    {
        // var manager = await context.Managers.FindAsync(request.ManagerId);
        // if (manager == null) throw new AppException("PokerManager not found");
        //
        // var fileType = walletTransactionType == WalletTransactionType.Income ? "Venda" : "Compra";
        //
        // var excel = new Excel(request.ManagerId, request.PostFile.FileName, fileType)
        // {
        //     CreatedAt = DateTime.Now,
        //     ManagerId = request.ManagerId,
        //     FileName = request.PostFile.FileName,
        //     FileType = fileType
        // };
        //
        // var rows = ReadExcelFile(request.PostFile,
        //     new List<(int, string)>
        //         { (1, "WalletIdentifier"), (2, "Value"), (3, "AssetWallet"), (6, "CreatedAt"), (8, "Description") });
        //
        // foreach (var row in rows)
        // {
        //     var fileNickname = row.FirstOrDefault(x => x.Name == "WalletIdentifier").Value;
        //     var fileCoins = decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value, new CultureInfo("pt-BR"));
        //     var fileWallet = row.FirstOrDefault(x => x.Name == "AssetWallet").Value;
        //     
        //     var fileDate = row.FirstOrDefault(x => x.Name == "CreatedAt").Value;
        //     var formats = new string[]{ "d/M/yyyy H:mm", "d/M/yy H:mm", "d/M/yy HH:mm", "d/M/yyyy HH:mm" };
        //
        //     if (DateTime.TryParseExact(fileDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
        //             out var parsedDate))
        //         parsedDate = parsedDate;
        //     else
        //         throw new FormatException($"Unable to parse date: {fileDate}");
        //
        //     var fileDescription = row.FirstOrDefault(x => x.Name == "Description").Value;
        //     
        //     var excelTransaction = new ExcelTransaction
        //     {
        //         Date = parsedDate,
        //         Coins = fileCoins,
        //         ManagerId = excel.ManagerId,
        //         WalletTransactionType = walletTransactionType,
        //         ExcelNickname = fileNickname,
        //         ExcelWallet = fileWallet,
        //     };
        //     
        //     excelTransaction.Description = fileDescription;
        //
        //     if (!context.ExcelTransactions.Any(x =>
        //             x.Date == excelTransaction.Date && x.Coins == excelTransaction.Coins &&
        //             x.WalletTransactionType == excelTransaction.WalletTransactionType &&
        //             x.ManagerId == excelTransaction.ManagerId)) excel.ExcelTransactions.Add(excelTransaction);
        // }
        //
        // if (excel.ExcelTransactions.Count == 0) throw new AppException("No transactions found");
        //
        // await context.Excels.AddAsync(excel);
        // await context.SaveChangesAsync();
        //
        // return mapper.Map<List<ExcelTransactionResponse>>(excel.ExcelTransactions);
        await Task.Yield();
        return null;
    }

    public async Task<List<ExcelTransactionResponse>> ImportTransferTransactions(ExcelRequest request)
    {
        // var manager = await context.Managers.FindAsync(request.ManagerId);
        // if (manager == null) throw new AppException("PokerManager not found");
        //
        // var fileType = "Transferência";
        // var excel = new Excel(request.ManagerId, request.PostFile.FileName, fileType)
        // {
        //     CreatedAt = DateTime.Now,
        //     ManagerId = request.ManagerId,
        //     FileName = request.PostFile.FileName,
        //     FileType = fileType
        // };
        //
        // var rows = ReadExcelFile(request.PostFile,
        //     new List<(int, string)> { (1, "From"), (2, "To"), (3, "CreatedAt"), (4, "Value"), (5, "Description") });
        //
        // foreach (var row in rows)
        // {
        //     var fileCoins =
        //         decimal.Parse(row.FirstOrDefault(x => x.Name == "Value").Value, new CultureInfo("pt-BR"));
        //     var fileWallet = "interna cred";
        //         
        //     var walletTransactionType =
        //         fileCoins > 0 ? WalletTransactionType.Expense : WalletTransactionType.Income;
        //     var fileNickname =
        //         fileCoins > 0 ? row.FirstOrDefault(x => x.Name == "From").Value : row.FirstOrDefault(x => x.Name == "To").Value;
        //
        //     fileCoins = fileCoins > 0
        //         ? fileCoins
        //         : decimal.Negate(fileCoins);
        //
        //     var fileDate = row.FirstOrDefault(x => x.Name == "CreatedAt").Value;
        //     var formats = new string[]{ "d/M/yyyy H:mm", "d/M/yy H:mm", "d/M/yy HH:mm", "d/M/yyyy HH:mm" };
        //
        //     if (DateTime.TryParseExact(fileDate, formats, CultureInfo.InvariantCulture, DateTimeStyles.None,
        //             out var parsedDate))
        //         parsedDate = parsedDate;
        //     else
        //         throw new FormatException($"Unable to parse date: {fileDate}");
        //
        //     var fileDescription = row.FirstOrDefault(x => x.Name == "Description").Value;
        //
        //     var excelTransaction = new ExcelTransaction
        //     {
        //         Date = parsedDate,
        //         Coins = fileCoins,
        //         ManagerId = excel.ManagerId,
        //         WalletTransactionType = walletTransactionType,
        //         ExcelNickname = fileNickname,
        //         ExcelWallet = fileWallet,
        //     };
        //     excelTransaction.Description = fileDescription;
        //
        //     if (!context.ExcelTransactions.Any(x =>
        //             x.Date == excelTransaction.Date && x.Coins == excelTransaction.Coins &&
        //             x.WalletTransactionType == excelTransaction.WalletTransactionType &&
        //             x.ManagerId == excelTransaction.ManagerId)) excel.ExcelTransactions.Add(excelTransaction);
        // }
        //
        // if (excel.ExcelTransactions.Count == 0) throw new AppException("No transactions found");
        //
        // await context.Excels.AddAsync(excel);
        // await context.SaveChangesAsync();
        //
        // return mapper.Map<List<ExcelTransactionResponse>>(excel.ExcelTransactions);
        await Task.Yield();
        return null;
    }

    private static List<List<(string Value, string Name)>> ReadExcelFile(IFormFile file, List<(int Column, string Name)> fields)
    {
        var rows = new List<List<(string Value, string Name)>>();

        using (var stream = file.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
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
            }
        }

        return rows;
    }

    public async Task<List<DigitalAssetTransactionResponse>> Reconciliation(Guid from, Guid to)
    {
        // var list = new List<DigitalAssetTransaction>();
        //
        // var walletTransactionFrom = context.WalletTransactions.FirstOrDefault(x => x.Id == from);
        // if (walletTransactionFrom == null) throw new AppException("From not found");
        // if (walletTransactionFrom.WalletId != null) throw new AppException("Já vinculado com algum lançamento manual");
        // if (walletTransactionFrom.NicknameId != null)
        //     throw new AppException("Já vinculado com algum lançamento manual");
        //
        // var walletTransactionTo = context.WalletTransactions.FirstOrDefault(x => x.Id == to);
        // if (walletTransactionTo == null) throw new AppException("To not found");
        // if (walletTransactionTo.LinkedToId.HasValue)
        //     throw new AppException("Esta transação manual já foi vinculada a uma transação OFX.");
        //
        // if (walletTransactionTo.WalletTransactionType != walletTransactionFrom.WalletTransactionType)
        //     throw new AppException("Transações de tipos diferentes.");
        //
        // if (walletTransactionTo.Coins != walletTransactionFrom.Coins)
        //     throw new AppException("Transações de valores de fichas diferentes.");
        //
        // walletTransactionTo.LinkedToId = walletTransactionFrom.Id;
        // walletTransactionTo.ApprovedAt = DateTime.Now;
        //
        // context.WalletTransactions.Update(walletTransactionTo);
        //
        // walletTransactionFrom.ApprovedAt = DateTime.Now;
        // walletTransactionFrom.NicknameId = walletTransactionTo.NicknameId;
        // walletTransactionFrom.WalletId = walletTransactionTo.WalletId;
        //
        // context.WalletTransactions.Update(walletTransactionTo);
        //
        // list.AddRange(walletTransactionFrom, walletTransactionTo);
        //
        // await context.SaveChangesAsync();
        //
        // return mapper.Map<List<DigitalAssetTransactionResponse>>(list);
        await Task.Yield();
        return null;
    }
}