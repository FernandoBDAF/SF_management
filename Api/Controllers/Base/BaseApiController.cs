using SFManagement.Application.DTOs.Common;
using SFManagement.Application.DTOs.AssetHolders;
using SFManagement.Application.DTOs.Assets;
using SFManagement.Application.Services.Base;
using SFManagement.Application.Services.Assets;
﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SFManagement.Domain.Common;
using SFManagement.Application.Services;
using SFManagement.Application.DTOs;

namespace SFManagement.Api.Controllers.Base;

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
    
    // public BaseApiController(BaseService<TEntity> service, IMapper mapper, BaseService<AssetPool> AssetPoolService, 
    // BaseService<WalletIdentifier> walletIdentifierService)
    // {
    //     _service = service;
    //     _mapper = mapper;
    // }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> Get()
    {
        var entities = await _service.List();
        var response = _mapper.Map<List<TResponse>>(entities);
        return Ok(response);
    }

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Get(Guid id)
    {
        var entity = await _service.Get(id);
        if (entity == null)
            return NotFound();
        
        var response = _mapper.Map<TResponse>(entity);
        return Ok(response);
    }

    [HttpDelete]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return NoContent();
    }

    [HttpPost]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public virtual async Task<IActionResult> Post(TRequest model)
    {
        var entity = _mapper.Map<TEntity>(model);
        var result = await _service.Add(entity);
        var response = _mapper.Map<TResponse>(result);
        return CreatedAtAction(nameof(Get), new { id = response.Id }, response);
    }

    [HttpPut]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Put(Guid id, TRequest model)
    {
        var entity = _mapper.Map<TEntity>(model);
        var result = await _service.Update(id, entity);
        var response = _mapper.Map<TResponse>(result);
        return Ok(response);
    }
}