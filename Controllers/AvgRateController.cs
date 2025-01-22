using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers
{
    public class AvgRateController : BaseApiController<AvgRate, AvgRateRequest, AvgRateResponse>
    {
        private readonly AvgService _avgService;

        public AvgRateController(BaseService<AvgRate> service, IMapper mapper, AvgService avgService) : base(service, mapper)
        {
            _avgService = avgService;
        }

        [HttpGet]
        [Route("reset/{managerId}")]
        public async Task Reset(Guid managerId)
        {
            await _avgService.Reset(managerId);
        }
    }
}
