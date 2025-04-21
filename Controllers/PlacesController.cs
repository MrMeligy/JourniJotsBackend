using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlacesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PlacesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetActivitesByCity")]
        public async Task<IActionResult> GetActivitesByCity(string city)
        {
            var activities = await _context.Activities
                .Where(a => a.City == city)
                .ToListAsync();

            if (activities == null || activities.Count == 0)
            {
                return NotFound("No activities found for the specified city.");
            }

            return Ok(activities);
        }

        [HttpGet("GetRestaurantsByCity")]
        public async Task<IActionResult> GetRestaurantsByCity(string city)
        {
            var restaurants = await _context.Restaurants
                .Where(a => a.City == city)
                .ToListAsync();

            if (restaurants == null || restaurants.Count == 0)
            {
                return NotFound("No restaurants found for the specified city.");
            }

            return Ok(restaurants);
        }

        [HttpGet("GetHotelsByCity")]
        public async Task<IActionResult> GetHotelsByCity(string city)
        {
            var hotels = await _context.Hotels
                .Where(a => a.City == city)
                .ToListAsync();

            if (hotels == null || hotels.Count == 0)
            {
                return NotFound("No restaurants found for the specified city.");
            }

            return Ok(hotels);
        }
        private bool ValidateUser()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return false;
            }
            return true;
        }
    }
}
