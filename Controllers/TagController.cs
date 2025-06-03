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
    public class TagController : BaseApiController<Tag, TagRequest, TagResponse>
    {
        private readonly TagService _tagService;
        private readonly IMapper _mapper;
        private TransactionService _transactionService;

        public TagController(TagService tagService, BaseService<Tag> service, IMapper mapper, TransactionService transactionService) : base(service, mapper)
        {
            _tagService = tagService;
            _mapper = mapper;
            _transactionService = transactionService;
        }

        public override async Task<List<TagResponse>> Get() => _mapper.Map<List<TagResponse>>(await _tagService.List());

        [HttpGet]
        [Route("balance/{tagId}")]
        public async Task<BalanceResponse> Balance(Guid tagId) => await _tagService.GetBalance(tagId);

        [HttpGet]
        [Route("transactions/{tagId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, DateTime? startDate = null, DateTime? endDate = null, int? quantity = 100, int? page = 0) => await _transactionService.GetTagTransactions(tagId, startDate, endDate, quantity.Value, page.Value);

        [HttpGet]
        [Route("transactions/{tagId}/{quantity?}/{page?}")]
        public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, int? quantity = 100, int? page = 0) => await _transactionService.GetTagTransactions(tagId, null, null, quantity.Value, page.Value);
    }
}
