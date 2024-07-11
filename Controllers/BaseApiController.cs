using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;
using SFManagement.Services;

namespace SFManagement.Controllers
{
    public class BaseApiController<T> : ControllerBase where T : BaseDomain
    {
        private readonly BaseService<T> _service;

        public BaseApiController(BaseService<T> service)
        {
            _service = service;
        }

        [HttpGet]
        public virtual async Task<List<T>> Get() => await _service.List();

        [HttpGet]
        [Route("{id}")]
        public virtual async Task<T?> Get(Guid id) => await _service.Get(id);

        [HttpDelete]
        [Route("{id}")]
        public virtual async Task Delete(Guid id) => await _service.Delete(id);

        [HttpPost]
        [Route("")]
        public virtual async Task<T> Post(T model) => await _service.Add(model);

        [HttpPut]
        [Route("{id}")]
        public virtual async Task<T> Put(T model) => await _service.Update(model);
    }
}
