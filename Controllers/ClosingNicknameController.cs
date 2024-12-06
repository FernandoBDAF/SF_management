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
        public ClosingNicknameController(BaseService<ClosingNickname> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
