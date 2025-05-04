using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;
using WebApplication2.Repositories;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/clients")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientRepository _clientRepository;

        public ClientsController(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            var trips = await _clientRepository.GetClientTripsAsync(id);
            if (trips == null)
                return NotFound("No trips found.");
            return Ok(trips);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient([FromBody] ClientDTO client)
        {
            var newId = await _clientRepository.CreateClientAsync(client);
            return Created($"api/clients/{newId}", new { IdClient = newId });
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientToTrip(int id, int tripId)
        {
            await _clientRepository.RegisterClientToTripAsync(id, tripId);
            return Ok("Client registered to trip.");
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> DeleteClientTrip(int id, int tripId)
        {
            await _clientRepository.DeleteClientTripAsync(id, tripId);
            return Ok("Registration deleted.");
        }
    }
}
