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
        public ClosingManagerController(BaseService<ClosingManager> service, IMapper mapper) : base(service, mapper)
        {
        }
    }
}
