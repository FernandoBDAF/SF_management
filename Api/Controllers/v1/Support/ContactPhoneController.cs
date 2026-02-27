using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Api.Controllers.Base;
using SFManagement.Application.DTOs.Support;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Support;
using SFManagement.Domain.Entities.Support;
using SFManagement.Infrastructure.Authorization;

namespace SFManagement.Api.Controllers.v1.Support;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[RequirePermission(Auth0Permissions.ReadClients)]
public class ContactPhoneController(ContactPhoneService service, IMapper mapper)
    : BaseApiController<ContactPhone, ContactPhoneRequest, ContactPhoneResponse>(service, mapper)
{
    private readonly ContactPhoneService _contactPhoneService = service;

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Post(ContactPhoneRequest model)
    {
        return base.Post(model);
    }

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Put(Guid id, ContactPhoneRequest model)
    {
        return base.Put(id, model);
    }

    [RequireRole(Auth0Roles.Admin)]
    public override Task<IActionResult> Delete(Guid id)
    {
        return base.Delete(id);
    }
}