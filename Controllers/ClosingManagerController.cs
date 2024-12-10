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
    public class ClosingManagerController : BaseApiController<ClosingManager, ClosingManagerRequest, ClosingManagerResponse>
    {
        private readonly ClosingManagerService _closingManagerService;
        private readonly IMapper _mapper;

        public ClosingManagerController(ClosingManagerService closingManagerService, BaseService<ClosingManager> service, IMapper mapper) : base(service, mapper)
        {
            _mapper = mapper;
            _closingManagerService = closingManagerService;
        }

        [HttpPut]
        [Route("done/{closingManagerId}")]
        public async Task<ClosingManagerResponse> Done(Guid closingManagerId) => _mapper.Map<ClosingManagerResponse>(await _closingManagerService.Done(closingManagerId));
    }
}
