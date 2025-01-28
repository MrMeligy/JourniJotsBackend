using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("UserRegister")]
        public async Task<IActionResult> CreateUserAsync([FromBody] CreatingUserDto dto)
        {
            var user = new User
            {
                UserName = dto.UserName,
                Email = dto.Email,
                Password = dto.Password,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                City = dto.City,
                DateOfBirth = dto.DateOfBirth,
            };
            if (dto.Fav_Food != null)
            {
                user.FavFood = dto.Fav_Food;
            }
            await _context.Users.AddAsync(user);
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpPost("Interests")]
        public async Task<IActionResult> AddInterestsAsync(int userId, int activityId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var activity = await _context.Activities.FirstOrDefaultAsync(x => x.Id == activityId);
            if (user == null || activity == null)
            {
                return NotFound();
            }
            var interest = new Interests
            {
                UserId = userId,
                ActivityId = activityId,
            };
            await _context.Interests.AddAsync(interest);
            _context.SaveChanges();
            return Ok(interest);
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("Follow")]
        public async Task<IActionResult> FollowAsync(int userId, int followedId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var followed = await _context.Users.FirstOrDefaultAsync(x => x.Id == followedId);
            if (user == null || followed == null)
            {
                return NotFound();
            }
            var follow = new Follow
            {
                UserId1 = userId,
                UserId2 = followedId,
            };
            await _context.Followings.AddAsync(follow);
            _context.SaveChanges();
            return Ok(follow);
        }
        [HttpPost("Unfollow")]
        public async Task<IActionResult> UnfollowAsync(int userId, int followedId)
        {
            var follow = await _context.Followings.FirstOrDefaultAsync(x => x.UserId1 == userId && x.UserId2 == followedId);
            if (follow == null)
            {
                return NotFound();
            }
            _context.Followings.Remove(follow);
            _context.SaveChanges();
            return Ok(follow);
        }
        [HttpPost("RateRestaurant")]
        public async Task<IActionResult> RateAsync(int userId, int restId, int rate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var rest = await _context.Restaurants.FirstOrDefaultAsync(x => x.Id == restId);
            if (user == null || rest == null)
            {
                return NotFound();
            }
            rest.Rating = (rest.Rating * rest.RatingCount + rate) / (rest.RatingCount + 1);
            rest.RatingCount++;
            _context.SaveChanges();
            return Ok(rest);
        }
        [HttpPost("RateHotel")]
        public async Task<IActionResult> RateHotelAsync(int userId, int hotelId, int rate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == hotelId);
            if (user == null || hotel == null)
            {
                return NotFound();
            }
            hotel.Rating = (hotel.Rating * hotel.RatingCount + rate) / (hotel.RatingCount + 1);
            hotel.RatingCount++;
            _context.SaveChanges();
            return Ok(hotel);
        }
        [HttpPost("RateActivity")]
        public async Task<IActionResult> RateActivityAsync(int userId, int actId, int rate)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var act = await _context.Activities.FirstOrDefaultAsync(x => x.Id == actId);
            if (user == null || act == null)
            {
                return NotFound();
            }
            act.Rating = (act.Rating * act.RatingCount + rate) / (act.RatingCount + 1);
            act.RatingCount++;
            _context.SaveChanges();
            return Ok(act);
        }

        [HttpPatch("UpdateUser")]
        public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] CreatingUserDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            if (dto.UserName != null)
            {
                user.UserName = dto.UserName;
            }
            if (dto.Email != null)
            {
                user.Email = dto.Email;
            }
            if (dto.Password != null)
            {
                user.Password = dto.Password;
            }
            if (dto.UserName != null)
            {
                user.UserName = dto.UserName;
            }
            if (dto.City != null)
            {
                user.City = dto.City;
            }

            _context.SaveChanges();
            return Ok(user);
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }
        [HttpGet("UserById")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            var user = await _context.Users
        .Include(u => u.Followed) // Include the Followed collection
        .FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            // Get the number of followers
            int numberOfFollowers = user.Followed.Count;

            // Return the user along with the number of followers
            var response = new
            {
                User = user,
                NumberOfFollowers = numberOfFollowers
            };

            return Ok(response);
        }

        [HttpGet("SearchUser")]
        public async Task<IActionResult> GetUserByNameAsync(string name)
        {
            var users = await _context.Users
            .Where(u => EF.Functions.Like(u.UserName, $"%{name}%"))
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.City,
            })
            .ToListAsync();
            return Ok(users);
        }
        [HttpGet("SearchRestaurant")]
        public async Task<IActionResult> GetRest(string name)
        {
            var rests = await _context.Restaurants
                .Where(r => EF.Functions.Like(r.Name, $"%{name}%"))
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Rating,
                    r.RatingCount,
                    r.Longitude,
                    r.Latitude,
                    r.Category
                }).ToListAsync();
            return Ok(rests);
        }

        [HttpGet("SearchHotel")]
        public async Task<IActionResult> GetHotel(string name)
        {
            var hotels = await _context.Hotels
                .Where(r => EF.Functions.Like(r.Name, $"%{name}%"))
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Rating,
                    r.RatingCount,
                    r.Longitude,
                    r.Latitude
                }).ToListAsync();
            return Ok(hotels);
        }
        [HttpGet("SearchActivit")]
        public async Task<IActionResult> GetActivity(string name)
        {
            var activities = await _context.Activities
                .Where(r => EF.Functions.Like(r.Name, $"%{name}%"))
                .Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Rating,
                    r.RatingCount,
                    r.Longitude,
                    r.Latitude
                }).ToListAsync();
            return Ok(activities);
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUserASync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok(user);
        }

    }
}
