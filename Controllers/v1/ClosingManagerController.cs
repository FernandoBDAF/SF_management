using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("api/v{verion:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ClosingManagerController : BaseApiController<ClosingManager, ClosingManagerRequest, ClosingManagerResponse>
    {
        private readonly ClosingManagerService _closingManagerService;
        private readonly IMapper _mapper;
        private readonly TransactionService _transactionService;

        public ClosingManagerController(ClosingManagerService closingManagerService, BaseService<ClosingManager> service, IMapper mapper, TransactionService transactionService) : base(service, mapper)
        {
            _mapper = mapper;
            _closingManagerService = closingManagerService;
            _transactionService = transactionService;
        }

        [HttpPut]
        [Route("done/{closingManagerId}")]
        public async Task<ClosingManagerResponse> Done(Guid closingManagerId) => _mapper.Map<ClosingManagerResponse>(await _closingManagerService.Done(closingManagerId));

        [HttpGet]
        [Route("transactions/{closingManagerId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid closingManagerId, DateTime? startDate = null, DateTime? endDate = null, int quantity = 100, int page = 0) => await _transactionService.GetClosingManagerTransactions(closingManagerId, startDate, endDate, quantity, page);

        [HttpGet]
        [Route("transactions/{closingManagerId}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> Transactions(Guid closingManagerId, int quantity = 100, int page = 0) => await _transactionService.GetClosingManagerTransactions(closingManagerId, null, null, quantity, page);
    }
}
