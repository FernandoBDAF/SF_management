using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFManagement.ViewModels;
using SFManagement.Services;

namespace SFManagement.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ClosingNicknameController : ControllerBase
    {
        private readonly ClosingNicknameService _closingNicknameService;
        private readonly IMapper _mapper;

        public ClosingNicknameController(ClosingNicknameService closingNicknameService, IMapper mapper)
        {
            _closingNicknameService = closingNicknameService;
            _mapper = mapper;
        }

        // Define the grouped response class within the controller
        public class GroupedClosingNicknameResponse
        {
            public Guid Key { get; set; }
            public List<ClosingNicknameResponse> Items { get; set; }
        }

        [HttpGet]
        [Route("closing-manager/{closingManagerId}")]
        public async Task<List<GroupedClosingNicknameResponse>> GetByClosingManagerId(Guid closingManagerId)
        {
            var groupedData = await _closingNicknameService.GetByClosingManagerId(closingManagerId);

            var response = groupedData.Select(g => new GroupedClosingNicknameResponse
            {
                Key = g.Key,
                Items = g.Select(nickname => _mapper.Map<ClosingNicknameResponse>(nickname)).ToList()
            }).ToList();

            return response;
        }
    }
}
