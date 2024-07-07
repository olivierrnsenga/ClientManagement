using ClientsManager.Data;
using Microsoft.AspNetCore.Mvc;

namespace ClientsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly ClientDataAccess _dataAccess;

        public ClientsController(ClientDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpGet]
        public IEnumerable<Client> GetClients()
        {
            return _dataAccess.GetClients();
        }

        [HttpGet("{id}")]
        public ActionResult<Client> GetClient(int id)
        {
            var client = _dataAccess.GetClientById(id);

            if (client == null)
            {
                return NotFound();
            }

            return client;
        }

        [HttpPost]
        public IActionResult AddClient(Client client)
        {
            _dataAccess.AddClient(client);
            return CreatedAtAction(nameof(GetClient), new { id = client.ClientId }, client);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateClient(int id, Client client)
        {
            if (id != client.ClientId)
            {
                return BadRequest();
            }

            var existingClient = _dataAccess.GetClientById(id);
            if (existingClient == null)
            {
                return NotFound();
            }

            _dataAccess.UpdateClient(client);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteClient(int id)
        {
            var client = _dataAccess.GetClientById(id);
            if (client == null)
            {
                return NotFound();
            }

            _dataAccess.DeleteClient(id);
            return NoContent();
        }
    }
}