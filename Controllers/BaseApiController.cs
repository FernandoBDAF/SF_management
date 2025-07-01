using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;
using SFManagement.ViewModels;
using SFManagement.Models.Entities;

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
    
    public BaseApiController(BaseService<TEntity> service, IMapper mapper, BaseService<AssetWallet> assetWalletService, 
    BaseService<WalletIdentifier> walletIdentifierService)
    {
        _service = service;
        _mapper = mapper;
    }

    [HttpGet]
    public virtual async Task<List<TResponse>> Get()
    {
        var entities = await _service.List();
        return _mapper.Map<List<TResponse>>(entities);
    }

    [HttpGet]
    [Route("{id}")]
    public virtual async Task<TResponse?> Get(Guid id)
    {
        var entity = await _service.Get(id);
        return entity == null ? null : _mapper.Map<TResponse>(entity);
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
        var entity = _mapper.Map<TEntity>(model);
        var result = await _service.Add(entity);
        return _mapper.Map<TResponse>(result);
    }

    [HttpPut]
    [Route("{id}")]
    public virtual async Task<TResponse> Put(Guid id, TRequest model)
    {
        var entity = _mapper.Map<TEntity>(model);
        var result = await _service.Update(id, entity);
        return _mapper.Map<TResponse>(result);
    }
}