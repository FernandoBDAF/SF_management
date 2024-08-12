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
    public class NicknameController : BaseApiController<Nickname, NicknameRequest, NicknameResponse>
    {
        public NicknameController(BaseService<Nickname> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
