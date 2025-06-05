using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Enums;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1
{
    [ApiController]
    [Route("api/v{verion:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ExcelController : BaseApiController<Excel, ExcelRequest, ExcelResponse>
    {
        private readonly ExcelService _excelService;

        public ExcelController(BaseService<Excel> service, IMapper mapper, ExcelService excelService) : base(service, mapper)
        {
            _excelService = excelService;
        }

        [HttpPost]
        [Route("import-buy-transactions")]
        public async Task<List<WalletTransactionResponse>> ImportBuyTransactions(ExcelRequest request) => await _excelService.ImportBuySellTransactions(request, WalletTransactionType.Expense);

        [HttpPost]
        [Route("import-sell-transactions")]
        public async Task<List<WalletTransactionResponse>> ImportSellTransactions(ExcelRequest request) => await _excelService.ImportBuySellTransactions(request, WalletTransactionType.Income);

        [HttpPost]
        [Route("import-transfer-transactions")]
        public async Task<List<WalletTransactionResponse>> ImportTransferTransactions(ExcelRequest request) => await _excelService.ImportTransferTransactions(request);

        [HttpPut]
        [Route("{from}/reconciliation/{to}")]
        public async Task<List<WalletTransactionResponse>> Reconciliation(Guid from, Guid to) => await _excelService.Reconciliation(from, to);

    }
}
