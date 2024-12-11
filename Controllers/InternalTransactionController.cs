using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Packaging.Ionic.Zlib;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class InternalTransactionController : BaseApiController<InternalTransaction, InternalTransactionRequest, InternalTransactionResponse>
    {
        private InternalTransactionService _internalTransactionService;
        private IMapper _mapper;
        private TransactionService _transactionService;


        public InternalTransactionController(BaseService<InternalTransaction> service, IMapper mapper, InternalTransactionService internalTransactionService, TransactionService transactionService) : base(service, mapper)
        {
            _internalTransactionService = internalTransactionService;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        [HttpPost]
        [Route("transfer/{toId}/{fromId}")]
        public async Task<List<InternalTransactionResponse>> Transfer(Guid toId, Guid fromId, [FromBody] InternalTransactionTransferRequest model) => _mapper.Map<List<InternalTransactionResponse>>(await _internalTransactionService.Transfer(toId, fromId, model));

        [HttpGet]
        [Route("transactions/{tagId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, DateTime? startDate = null, DateTime? endDate = null, int? quantity = 100, int? page = 0) => await _transactionService.GetInternalTransactions(tagId, startDate, endDate, quantity.Value, page.Value);
    }
}
