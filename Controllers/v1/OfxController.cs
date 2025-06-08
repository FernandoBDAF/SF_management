using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{verion:apiVersion}/[controller]")]
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
    public override async Task<List<OfxResponse>> Get()
    {
        return _mapper.Map<List<Ofx>, List<OfxResponse>>(await _ofxService.List());
    }

    public override async Task<OfxResponse> Post(OfxRequest model)
    {
        return _mapper.Map<OfxResponse>(await _ofxService.Add(model.PostFile, model.BankId));
    }

    public override Task<OfxResponse> Put(Guid id, OfxRequest model)
    {
        throw new NotImplementedException("Transação não permitida");
    }
}