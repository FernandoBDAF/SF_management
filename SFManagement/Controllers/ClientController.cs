using Microsoft.AspNetCore.Mvc;
using SFManagement.Models;

namespace SFManagement.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ILogger<ClientController> _logger;

        public ClientController(ILogger<ClientController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public List<Client> Get() => new List<Client>();

        [HttpGet]
        [Route("{id}")]
        public Client Get(Guid id) => new Client();

        [HttpDelete]
        [Route("{id}")]
        public Client Delete(Guid id) => new Client();

        [HttpPost]
        [Route("")]
        public Client Post(Client model) => model;

        [HttpPut]
        [Route("{id}")]
        public Client Put(Guid id, Client model) => model;
    }
}
