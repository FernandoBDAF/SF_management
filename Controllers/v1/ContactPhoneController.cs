using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Support;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class ContactPhoneController(ContactPhoneService service, IMapper mapper)
    : BaseApiController<ContactPhone, ContactPhoneRequest, ContactPhoneResponse>(service, mapper)
{
    private readonly ContactPhoneService _contactPhoneService = service;
}