using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1
{
    [ApiController]
    [Route("api/v{verion:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class NicknameController : BaseApiController<Nickname, NicknameRequest, NicknameResponse>
    {
        private readonly NicknameService _nicknameService;
        
        private readonly IMapper _mapper;
        public NicknameController(BaseService<Nickname> service, IMapper mapper, NicknameService nicknameService) : base(service, mapper)
        {
            _nicknameService = nicknameService;
            _mapper = mapper;
        }
        
        [HttpGet("/nickname-client/{clientId}")]
        public async Task<List<NicknameResponse>> GetNicknames(Guid clientId) => _mapper.Map<List<NicknameResponse>>(await _nicknameService.GetByClientId(clientId));
    }
}
