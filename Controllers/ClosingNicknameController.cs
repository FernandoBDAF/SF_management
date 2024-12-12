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
    public class ClosingNicknameController : BaseApiController<ClosingNickname, ClosingNicknameRequest, ClosingNicknameResponse>
    {
        private readonly IMapper _mapper;

        private readonly ClosingNicknameService _closingNicknameService;

        public ClosingNicknameController(BaseService<ClosingNickname> service, IMapper mapper, ClosingNicknameService closingNicknameService) : base(service, mapper)
        {
            _mapper = mapper;
            _closingNicknameService = closingNicknameService;
        }

        [HttpGet]
        [Route("closing-manager/{closingManagerId}")]
        public async Task<List<IGrouping<Guid, ClosingNicknameResponse>>> GetByClosingManagerId(Guid closingManagerId) => _mapper.Map<List<IGrouping<Guid, ClosingNicknameResponse>>>(await _closingNicknameService.GetByClosingManagerId(closingManagerId));
    }
}
