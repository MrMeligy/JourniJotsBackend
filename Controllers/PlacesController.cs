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

        [HttpGet("GetActivitiesByCity")]
        public async Task<IActionResult> GetActivitiesByCity(string city, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Activities
                .Where(a => a.City == city)
                .OrderByDescending(a => a.RatingCount);

            var totalCount = await query.CountAsync();

            var activities = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (activities.Count == 0)
            {
                return NotFound("No activities found for the specified city.");
            }
            bool hasNext = totalCount > pageNumber * pageSize;
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNext = hasNext,
                Data = activities
            });
        }


        [HttpGet("GetRestaurantsByCity")]
        public async Task<IActionResult> GetRestaurantsByCity(string city, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Restaurants
               .Where(r => r.City == city)
               .OrderByDescending(r => r.RatingCount); // الترتيب من الأعلى للأقل

            var totalCount = await query.CountAsync();

            var restaurants = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (restaurants.Count == 0)
            {
                return NotFound("No restaurats found for the specified city.");
            }
            bool hasNext = totalCount > pageNumber * pageSize;

            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                HasNext = hasNext,
                Data = restaurants
            });
        }

        [HttpGet("GetHotelsByCity")]
        public async Task<IActionResult> GetHotelsByCity(string city, int pageNumber = 1, int pageSize = 10)
        {
            var query = _context.Hotels
               .Where(h => h.City == city)
               .OrderByDescending(h => h.RatingCount); // الترتيب من الأعلى للأقل

            var totalCount = await query.CountAsync();

            var hotels = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            if (hotels.Count == 0)
            {
                return NotFound("No hotels found for the specified city.");
            }
            bool hasNext = totalCount > pageNumber * pageSize;
            return Ok(new
            {
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize, 
                HasNext= hasNext,
                Data = hotels
            });
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
