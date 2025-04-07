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
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var activities = await _context.Activities
                .Where(a => a.City == city)
                .ToListAsync();

            if (activities == null || activities.Count == 0)
            {
                return NotFound("No activities found for the specified city.");
            }

            return Ok(activities);
        }
    }
}
