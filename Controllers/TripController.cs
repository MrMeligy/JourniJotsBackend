using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public TripController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("CreateTrip")]
        public async Task<IActionResult> CreateTrip(int userId, String title)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            var trip = new Trip
            {
                UserId = userId,
                Title = title,
            };
            await _context.Trips.AddAsync(trip);
            _context.SaveChanges();
            return Ok(trip);
        }
        [HttpPost("AddActivity")]
        public async Task<IActionResult> AddActivity(int actId, int tripId)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
            var activity = await _context.Activities.FirstOrDefaultAsync(x => x.Id == actId);
            if (trip == null || activity == null)
            {
                return NotFound();
            }
            var tripActivity = new Trip_Activity
            {
                TripId = tripId,
                ActivityId = actId,
            };
            await _context.Trip_Activities.AddAsync(tripActivity);
            return Ok(tripActivity);
        }

        [HttpPost("AddHotel")]
        public async Task<IActionResult> AddHotel(int hotelId, int tripId)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == hotelId);
            if (trip == null || hotel == null)
            {
                return NotFound();
            }
            var tripHotel = new Trip_Hotel
            {
                TripId = tripId,
                HotelId = hotelId,
            };
            await _context.Trip_Hotels.AddAsync(tripHotel);
            return Ok(tripHotel);
        }

        [HttpPost("AddRestaurant")]
        public async Task<IActionResult> AddRestaurant(int restId, int tripId)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
            var rest = await _context.Activities.FirstOrDefaultAsync(x => x.Id == restId);
            if (trip == null || rest == null)
            {
                return NotFound();
            }
            var tripRest = new Trip_Restaurant
            {
                TripId = tripId,
                RestaurantId = restId,
            };
            await _context.Trip_Restaurants.AddAsync(tripRest);
            return Ok(tripRest);
        }
        [HttpGet("GetTrips")]
        public async Task<IActionResult> GetTrip(int userId)
        {
            var trip = await _context.Trips.Where(x => x.UserId == userId).Select(t => new
            {
                Trip = t,
                Activities = t.Activities
                .Select(ta => new
                {
                    ta.Activity.Id,
                    ta.Activity.Name,
                    ta.Activity.Rating,
                    ta.Activity.RatingCount
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
            })
        .ToListAsync();
            if (trip == null)
            {
                return NotFound();
            }

            return Ok(trip);
        }

        [HttpDelete("DeleteTrip")]
        public async Task<IActionResult> DeleteTrip(int tripId)
        {
            var trip = await _context.Trips.FirstOrDefaultAsync(x => x.Id == tripId);
            if (trip == null)
            {
                return NotFound();
            }
            _context.Trips.Remove(trip);
            _context.SaveChanges();
            return Ok();
        }
        [HttpDelete("DeleteActivity")]
        public async Task<IActionResult> DeleteActivity(int actId, int tripId)
        {
            var tripActivity = await _context.Trip_Activities.FirstOrDefaultAsync(x => x.ActivityId == actId && x.TripId == tripId);
            if (tripActivity == null)
            {
                return NotFound();
            }
            _context.Trip_Activities.Remove(tripActivity);
            _context.SaveChanges();
            return Ok();
        }
        [HttpDelete("DeleteHotel")]
        public async Task<IActionResult> DeleteHotel(int hotelId, int tripId)
        {
            var tripHotel = await _context.Trip_Hotels.FirstOrDefaultAsync(x => x.HotelId == hotelId && x.TripId == tripId);
            if (tripHotel == null)
            {
                return NotFound();
            }
            _context.Trip_Hotels.Remove(tripHotel);
            _context.SaveChanges();
            return Ok();
        }
        [HttpDelete("DeleteRestaurant")]
        public async Task<IActionResult> DeleteRestaurant(int restId, int tripId)
        {
            var tripRest = await _context.Trip_Restaurants.FirstOrDefaultAsync(x => x.RestaurantId == restId && x.TripId == tripId);
            if (tripRest == null)
            {
                return NotFound();
            }
            _context.Trip_Restaurants.Remove(tripRest);
            _context.SaveChanges();
            return Ok();
        }
    }
}
