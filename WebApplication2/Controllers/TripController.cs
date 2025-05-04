using Microsoft.AspNetCore.Mvc;
using WebApplication2.Repositories;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/trips")]
    public class TripsController : ControllerBase
    {
        private readonly ITripRepository _tripRepository;

        public TripsController(ITripRepository tripRepository)
        {
            _tripRepository = tripRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _tripRepository.GetTripsAsync();
            return Ok(trips);
        }
    }
}
