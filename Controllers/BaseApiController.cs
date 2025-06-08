using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;

namespace SFManagement.Controllers;

public class BaseApiController<TEntity, TRequest, TResponse> : ControllerBase where TEntity : BaseDomain
    where TRequest : class
    where TResponse : BaseResponse
{
    private readonly IMapper _mapper;
    private readonly BaseService<TEntity> _service;

    public BaseApiController(BaseService<TEntity> service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public virtual async Task<List<TResponse>> Get()
    {
        return _mapper.Map<List<TResponse>>(await _service.List());
    }

    [HttpGet]
    [Route("{id}")]
    public virtual async Task<TResponse?> Get(Guid id)
    {
        return _mapper.Map<TResponse>(await _service.Get(id));
    }

    [HttpDelete]
    [Route("{id}")]
    public virtual async Task Delete(Guid id)
    {
        await _service.Delete(id);
    }

    [HttpPost]
    [Route("")]
    public virtual async Task<TResponse> Post(TRequest model)
    {
        return _mapper.Map<TResponse>(await _service.Add(_mapper.Map<TEntity>(model)));
    }

    [HttpPut]
    [Route("{id}")]
    public virtual async Task<TResponse> Put(Guid id, TRequest model)
    {
        return _mapper.Map<TResponse>(await _service.Update(id, _mapper.Map<TEntity>(model)));
    }
}