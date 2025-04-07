using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpPost("Follow")]
        public async Task<IActionResult> FollowAsync(int followedId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {

                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var followed = await _context.Users.FirstOrDefaultAsync(x => x.Id == followedId);
                if (user == null || followed == null)
                {
                    return BadRequest("User Not Found");
                }
                if (userId == followedId)
                    return BadRequest("You Can't Follow Your Self");
                var existingFollow = await _context.Followings
                .FirstOrDefaultAsync(f => f.UserId1 == userId && f.UserId2 == followedId);

                if (existingFollow != null)
                {
                    _context.Followings.Remove(existingFollow);
                    _context.SaveChanges();
                    return Ok("You Don't Follow This User Now");
                }
                var follow = new Follow
                {
                    UserId1 = userId,
                    UserId2 = followedId,
                };
                await _context.Followings.AddAsync(follow);
                await _context.SaveChangesAsync();
                return Ok("You Follow This User");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException?.Message);
            }

        }

        [HttpPost("RateRestaurant")]
        public async Task<IActionResult> RateAsync([FromBody] RateDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var rest = await _context.Restaurants.FirstOrDefaultAsync(x => x.Id == dto.placeId);
                if (user == null || rest == null)
                {
                    return NotFound();
                }
                rest.Rating = (rest.Rating * rest.RatingCount + dto.rate) / (rest.RatingCount + 1);
                rest.RatingCount++;
                _context.SaveChanges();
                return Ok(new { restauran = rest.Name, rating = rest.Rating, ratingCount = rest.RatingCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message);
            }
        }
        [HttpPost("RateHotel")]
        public async Task<IActionResult> RateHotelAsync([FromBody] RateDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var hotel = await _context.Hotels.FirstOrDefaultAsync(x => x.Id == dto.placeId);
                if (user == null || hotel == null)
                {
                    return NotFound();
                }
                hotel.Rating = (hotel.Rating * hotel.RatingCount + dto.rate) / (hotel.RatingCount + 1);
                hotel.RatingCount++;
                _context.SaveChanges();
                return Ok(new { restauran = hotel.Name, rating = hotel.Rating, ratingCount = hotel.RatingCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message);
            }
        }

        [HttpPost("RateActivity")]
        public async Task<IActionResult> RateActivityAsync([FromBody] RateDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
                var act = await _context.Activities.FirstOrDefaultAsync(x => x.Id == dto.placeId);
                if (user == null || act == null)
                {
                    return NotFound();
                }
                act.Rating = (act.Rating * act.RatingCount + dto.rate) / (act.RatingCount + 1);
                act.RatingCount++;
                _context.SaveChanges();
                return Ok(new { restauran = act.Name, rating = act.Rating, ratingCount = act.RatingCount });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message);
            }
        }
        [HttpGet("GetUserProfile")]
        public async Task<IActionResult> GetUserProfileAsync(int userId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int parsedUserId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                var profile = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new
                    {
                        userId = u.Id,
                        userName = u.UserName,
                        profilePicture = u.ProfilePicture,
                        isFollowed = u.Followed.Any(f => f.UserId1 == parsedUserId),
                        Posts = u.Posts.Select(p => new
                        {
                            postId = p.Id,
                            post = p.Content,
                            likeCount = p.PostLikes.Count,
                            commentCount = p.PostComments.Count,
                            createdAt = p.CreatedAt,
                            isLikedByCurrentUser = p.PostLikes.Any(l => l.UserId == parsedUserId),
                            postImages = p.Images.Select(pi => new
                            {
                                pi.Id,
                                imageData = Convert.ToBase64String(pi.Image)
                            }).ToList()
                        }).ToList(),
                        interests = u.Intersts.Select(i => i.interst).ToList(),
                        trips = u.Trips.Select(t => new
                        {
                            city = t.City,
                            startDate = t.StartDate,
                        }).ToList()
                    })
                    .FirstOrDefaultAsync(); return Ok(profile);

                /*await _context.Posts.Where(x => x.UserId == userId).Select(p => new
            {
                UserName = p.User.UserName,
                ProfilePicture = p.User.ProfilePicture,
                PostId = p.Id,
                Post = p.Content,
                LikeCount = p.PostLikes.Count,
                CommentCount = p.PostComments.Count,
                CreatedAt = p.CreatedAt,
                IsLikedByCurrentUser = _context.PostLikes.Any(l => l.PostId == p.Id && l.UserId == parsedUserId),
                PostImages = p.Images.Select(pi => new
                {
                    pi.Id,
                    ImageData = Convert.ToBase64String(pi.Image)
                }).ToList()

            }
            ).ToListAsync();*/
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException });
            }
        }

        [HttpGet("AllUsers")]
        public async Task<IActionResult> GetAllUsersAsync()
        {
            var users = await _context.Users.Select(u => new
            {
                User = u,
                Followers = u.Followed.Count()
            }).ToListAsync();
            return Ok(users);
        }
        [HttpGet("UserById")]
        public async Task<IActionResult> GetUserByIdAsync(int id)
        {
            try
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
                List<Post> posts = _context.Posts.Where(p => p.UserId == id).ToList();
                // Return the user along with the number of followers
                var response = new
                {
                    ProfilePicture = user.ProfilePicture,
                    posts = posts,
                    NumberOfFollowers = numberOfFollowers
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.InnerException?.Message);
            }
        }

        [HttpGet("SearchUser")]
        public async Task<IActionResult> GetUserByNameAsync(string name)
        {
            try
            {
                var users = await _context.Users
                .Where(u => EF.Functions.Like(u.UserName, $"%{name}%"))
                .Select(u => new
                {
                    u.Id,
                    u.UserName,

                })
                .ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("SearchRestaurant")]
        public async Task<IActionResult> GetRest(string name)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("SearchHotel")]
        public async Task<IActionResult> GetHotel(string name)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("SearchActivity")]
        public async Task<IActionResult> GetActivity(string name)
        {
            try
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteUser")]
        public async Task<IActionResult> DeleteUserASync(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound("User Not Exist");
            }
            _context.Users.Remove(user);
            _context.SaveChanges();
            return Ok(user);
        }
        [HttpDelete("DeleteProfilePicture")]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return BadRequest("User Not Exists");
            if (user.ProfilePicture != null)
            {
                user.ProfilePicture = null;
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            else
                return BadRequest("There Is No Profile Picture");
        }

    }
}

