using AutoMapper;
using Azure;
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
    public class TagController : BaseApiController<Tag, TagRequest, TagResponse>
    {
        private readonly TagService _tagService;
        private readonly IMapper _mapper;

        public TagController(TagService tagService, BaseService<Tag> service, IMapper mapper) : base(service, mapper)
        {
            _tagService = tagService;
            _mapper = mapper;
        }

        public override async Task<List<TagResponse>> Get() => _mapper.Map<List<TagResponse>>(await _tagService.List());
    }
}
