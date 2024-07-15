using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OfxController : BaseApiController<Ofx, OfxRequest, OfxResponse>
    {
        private readonly ILogger<OfxController> _logger;

        public OfxController(BaseService<Ofx> service, ILogger<OfxController> logger, IMapper mapper) : base(service, mapper)
        {
            _logger = logger;
        }

        public override Task<OfxResponse> Put(Guid id, OfxRequest model)
        {
            throw new NotImplementedException("Transação não permitida");
        }
    }
}
