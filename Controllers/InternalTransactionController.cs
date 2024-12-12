using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        
        public InternalTransactionController(BaseService<InternalTransaction> service, IMapper mapper, InternalTransactionService internalTransactionService) : base(service, mapper)
        {
            _internalTransactionService = internalTransactionService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("transfer/{toId}/{fromId}")]
        public async Task<List<InternalTransactionResponse>> Transfer(Guid toId, Guid fromId, [FromBody] InternalTransactionTransferRequest model) => _mapper.Map<List<InternalTransactionResponse>>(await _internalTransactionService.Transfer(toId, fromId, model));
    }
}
