using AutoMapper;
using Azure;
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
    public class BankTransactionController : BaseApiController<BankTransaction, BankTransactionRequest, BankTransactionResponse>
    {
        private readonly BankTransactionService _bankTransactionService;
        private readonly IMapper _mapper;

        public BankTransactionController(BaseService<BankTransaction> service, IMapper mapper, BankTransactionService bankTransactionService) : base(service, mapper)
        {
            _bankTransactionService = bankTransactionService;
            _mapper = mapper;
        }

        [HttpGet]
		public override async Task<List<BankTransactionResponse>> Get() => _mapper.Map<List<BankTransactionResponse>>( _bankTransactionService.List());

		[HttpPut]
        [Route("approve/{bankTransactionId}")]
        public async Task<BankTransactionResponse> Approve(Guid bankTransactionId) => _mapper.Map<BankTransactionResponse>(await _bankTransactionService.Approve(bankTransactionId));

        [HttpPut]
        [Route("link/{fromBankTransactionId}/{toBankTransactionId}")]
        public async Task<BankTransactionResponse> Link(Guid fromBankTransactionId, Guid toBankTransactionId) => _mapper.Map<BankTransactionResponse>(await _bankTransactionService.Link(fromBankTransactionId, toBankTransactionId));

        [HttpGet]
        [Route("list/{clientId}/{bankId}")]
        public async Task<List<BankTransactionResponse>> ListByClienteId(Guid? clientId, Guid? bankId) => _mapper.Map<List<BankTransactionResponse>>(await _bankTransactionService.ListByClientIdAndBankId(clientId, bankId));

        //TODO: Criar um endpoint com datas.
    }
}
