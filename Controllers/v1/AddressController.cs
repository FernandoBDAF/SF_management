using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models.Support;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers.v1;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
public class AddressController(AddressService service, IMapper mapper)
    : BaseApiController<Address, AddressRequest, AddressResponse>(service, mapper)
{
    private readonly AddressService _addressService = service;
}