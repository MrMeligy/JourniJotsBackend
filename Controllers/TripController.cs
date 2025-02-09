using Backend.Dtos;
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
    public class TripController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TripController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("CreateTrip")]
        public async Task<IActionResult> CreateTrip([FromBody] String title)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                var trip = new Trip
                {
                    UserId = userId,
                    Title = title,
                };
                await _context.Trips.AddAsync(trip);
                await _context.SaveChangesAsync();
                var response = new
                {
                    UserId = trip.UserId,
                    UserName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    tripId = trip.Id,
                    Title = trip.Title
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpPost("AddActivity")]
        public async Task<IActionResult> AddActivity([FromBody] TripDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == dto.tripId);
            var activity = await _context.Activities.FirstOrDefaultAsync(x => x.Id == dto.placeId);
            if (trip == null || activity == null)
            {
                return NotFound();
            }
            try
            {
                var existed = await _context.Trip_Activities
                    .FirstOrDefaultAsync(t => t.TripId == dto.tripId && t.ActivityId == dto.placeId);
                if (existed != null)
                    return BadRequest("Activty Already Exist");
                var tripActivity = new Trip_Activity
                {
                    TripId = dto.tripId,
                    ActivityId = dto.placeId,
                };
                await _context.Trip_Activities.AddAsync(tripActivity);
                await _context.SaveChangesAsync();
                var response = new
                {
                    TripId = tripActivity.TripId,
                    ActivityId = tripActivity.ActivityId,
                    TripTitle = tripActivity.Trip.Title,
                    userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    Activity = tripActivity.Activity.Name
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }

        [HttpPost("AddHotel")]
        public async Task<IActionResult> AddHotel([FromBody] TripDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var existed = await _context.Trip_Hotels
                   .FirstOrDefaultAsync(t => t.TripId == dto.tripId && t.HotelId == dto.placeId);
            if (existed != null)
                return BadRequest("Hotel Already Exist");
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == dto.tripId);
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == dto.placeId);
            if (trip == null || hotel == null)
            {
                return NotFound();
            }
            try
            {
                var tripHotel = new Trip_Hotel
                {
                    TripId = dto.tripId,
                    HotelId = dto.placeId
                };
                await _context.Trip_Hotels.AddAsync(tripHotel);
                await _context.SaveChangesAsync();
                var response = new
                {
                    TripId = tripHotel.TripId,
                    HotelId = tripHotel.HotelId,
                    TripTitle = tripHotel.Trip.Title,
                    userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    Hotel = tripHotel.Hotel.Name
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }

        [HttpPost("AddRestaurant")]
        public async Task<IActionResult> AddRestaurant([FromBody] TripDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var existed = await _context.Trip_Restaurants
                   .FirstOrDefaultAsync(t => t.TripId == dto.tripId && t.RestaurantId == dto.placeId);
            if (existed != null)
                return BadRequest("Restaurant Already Exist");
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == dto.tripId);
            var rest = await _context.Restaurants.FirstOrDefaultAsync(x => x.Id == dto.placeId);
            if (trip == null || rest == null)
            {
                return NotFound();
            }
            try
            {
                var tripRest = new Trip_Restaurant
                {
                    TripId = dto.tripId,
                    RestaurantId = dto.placeId,
                };
                await _context.Trip_Restaurants.AddAsync(tripRest);
                await _context.SaveChangesAsync();
                var response = new
                {
                    TripId = tripRest.TripId,
                    RestaurantId = tripRest.RestaurantId,
                    TripTitle = tripRest.Trip.Title,
                    userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    Restaurant = tripRest.Restaurant.Name
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("GetTrips")]
        public async Task<IActionResult> GetTrip()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                var trip = await _context.Trips
                    .Where(x => x.UserId == userId)
                    .Include(t => t.Activities)
                    .Include(t => t.Restaurants)
                    .Include(t => t.Hotels)
                    .Select(t => new
                    {
                        TripTitle = t.Title,
                        Activities = t.Activities
                        .Select(ta => new
                        {
                            ta.Activity.Id,
                            ta.Activity.Name,
                            ta.Activity.Rating,
                            ta.Activity.RatingCount,
                            ta.Activity.Latitude,
                            ta.Activity.Longitude,
                        }).ToList(),
                        Restaurants = t.Restaurants
                        .Select(tr => new
                        {
                            tr.Restaurant.Id,
                            tr.Restaurant.Name,
                            tr.Restaurant.Category,
                            tr.Restaurant.Rating,
                            tr.Restaurant.RatingCount
                        }).ToList(),
                        Hotels = t.Hotels
                        .Select(th => new
                        {
                            th.Hotel.Id,
                            th.Hotel.Name,
                            th.Hotel.Rating,
                            th.Hotel.RatingCount
                        }).ToList()
                    }).ToListAsync();
                if (trip == null)
                {
                    return Ok("There is No Trips Yet");
                }

                return Ok(trip);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }

        [HttpPut("UpdateTitle")]
        public async Task<IActionResult> UpdateTitle(int tripId, [FromBody] string title)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var trip = await _context.Trips.FirstOrDefaultAsync(t => t.Id == tripId);
            if (trip == null)
                return NotFound("Trip Not Found");
            if (trip.UserId != userId)
                return BadRequest("Not Allowed");
            trip.Title = title;
            await _context.SaveChangesAsync();
            return Ok("Title Updated");

        }

        [HttpDelete("DeleteTrip")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
            if (trip == null)
            {
                return NotFound();
            }
            try
            {
                if (trip.UserId != userId)
                    return BadRequest("Trip Not Allowed");
                _context.Trips.Remove(trip);
                _context.SaveChanges();
                return Ok("Trip Is Deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpDelete("DeleteActivity")]
        public async Task<IActionResult> DeleteActivity([FromBody] TripDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var tripActivity = await _context.Trip_Activities
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(x => x.ActivityId == dto.placeId && x.TripId == dto.tripId);
            if (tripActivity == null)
            {
                return NotFound();
            }
            if (userId != tripActivity.Trip.UserId)
                return BadRequest("Not Allowed");
            try
            {
                _context.Trip_Activities.Remove(tripActivity);
                _context.SaveChanges();
                return Ok("Activity Deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpDelete("DeleteHotel")]
        public async Task<IActionResult> DeleteHotel([FromBody] TripDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var tripHotel = await _context.Trip_Hotels
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(x => x.HotelId == dto.placeId && x.TripId == dto.tripId);
            if (tripHotel == null)
            {
                return NotFound();
            }
            if (userId != tripHotel.Trip.UserId)
                return BadRequest("Not Allowed");
            try
            {
                _context.Trip_Hotels.Remove(tripHotel);
                _context.SaveChanges();
                return Ok("Hotel Deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpDelete("DeleteRestaurant")]
        public async Task<IActionResult> DeleteRestaurant([FromBody] TripDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var tripRest = await _context.Trip_Restaurants
                .Include(t => t.Trip)
                .FirstOrDefaultAsync(x => x.RestaurantId == dto.placeId && x.TripId == dto.tripId);
            if (tripRest == null)
            {
                return NotFound();
            }
            if (userId != tripRest.Trip.UserId)
                return BadRequest("Not Allowed");
            try
            {
                _context.Trip_Restaurants.Remove(tripRest);
                _context.SaveChanges();
                return Ok("Restaurant Deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
    }
}
