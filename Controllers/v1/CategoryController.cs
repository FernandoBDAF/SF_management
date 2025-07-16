using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Support;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class CategoryController : BaseApiController<Category, CategoryRequest, 
    CategoryResponse>
{
    private readonly IMapper _mapper;
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService category, 
        BaseService<Category> service, IMapper mapper) : base(service, mapper)
    {
        _categoryService = category;
        _mapper = mapper;
    }

    public override async Task<IActionResult> Get()
    {
        var categories = await _categoryService.List();
        var response = _mapper.Map<List<CategoryResponse>>(categories);
        return Ok(response);
    }

    // [HttpGet]
    // [Route("balance/{tagId}")]
    // public async Task<BalanceResponse> Balance(Guid tagId)
    // {
    //     return await _categoryService.GetBalance(tagId);
    // }

    // [HttpGet]
    // [Route("transactions/{tagId}/{startDate?}/{endDate?}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, DateTime? startDate = null,
    //     DateTime? endDate = null, int? quantity = 100, int? page = 0)
    // {
    //     return await _transactionService.GetTagTransactions(tagId, startDate, endDate, quantity.Value, page.Value);
    // }
    //
    // [HttpGet]
    // [Route("transactions/{tagId}/{quantity?}/{page?}")]
    // public async Task<TableResponse<TransactionResponse>> InternalTransactions(Guid tagId, int? quantity = 100,
    //     int? page = 0)
    // {
    //     return await _transactionService.GetTagTransactions(tagId, null, null, quantity.Value, page.Value);
    // }
}