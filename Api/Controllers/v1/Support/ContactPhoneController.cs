using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Support;
using SFManagement.Domain.Entities.Support;

namespace SFManagement.Api.Controllers.v1.Support;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ContactPhoneController(ContactPhoneService service, IMapper mapper)
    : BaseApiController<ContactPhone, ContactPhoneRequest, ContactPhoneResponse>(service, mapper)
{
    private readonly ContactPhoneService _contactPhoneService = service;
}