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
    public class OfxController : BaseApiController<Ofx, OfxRequest, OfxResponse>
    {
        private readonly ILogger<OfxController> _logger;
        private readonly OfxService _ofxService;
        private readonly IMapper _mapper;

        public OfxController(OfxService ofxService, BaseService<Ofx> service, ILogger<OfxController> logger, IMapper mapper) : base(service, mapper)
        {
            _logger = logger;
            _ofxService = ofxService;
            _mapper = mapper;
        }

        [HttpGet]
        public override async Task<List<OfxResponse>> Get() => _mapper.Map<List<Ofx>, List<OfxResponse>>(await _ofxService.List());

        public override async Task<OfxResponse> Post(OfxRequest model)
        {
            return _mapper.Map<OfxResponse>(await _ofxService.Add(model.PostFile, model.BankId));
        }

        public override Task<OfxResponse> Put(Guid id, OfxRequest model)
        {
            throw new NotImplementedException("Transação não permitida");
        }
    }
}
