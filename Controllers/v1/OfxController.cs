using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Transactions;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class OfxController : BaseApiController<Ofx, OfxRequest, OfxResponse>
{
    private readonly ILogger<OfxController> _logger;
    private readonly IMapper _mapper;
    private readonly OfxService _ofxService;

    public OfxController(OfxService ofxService, BaseService<Ofx> service, ILogger<OfxController> logger, IMapper mapper)
        : base(service, mapper)
    {
        _logger = logger;
        _ofxService = ofxService;
        _mapper = mapper;
    }

    [HttpGet]
    public override async Task<IActionResult> Get()
    {
        var ofxs = await _ofxService.List();
        var response = _mapper.Map<List<Ofx>, List<OfxResponse>>(ofxs);
        return Ok(response);
    }

    public override async Task<IActionResult> Post(OfxRequest model)
    {
        var result = await _ofxService.Add(model.PostFile, model.BankId);
        var response = _mapper.Map<OfxResponse>(result);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    public override Task<IActionResult> Put(Guid id, OfxRequest model)
    {
        return Task.FromResult<IActionResult>(BadRequest("Transação não permitida"));
    }
}