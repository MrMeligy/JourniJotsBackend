using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        public AccountController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        //Register Endpoint
        [HttpPost("Register")]
        public async Task<IActionResult> CreateUserAsync([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data", details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                // Check if user already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (existingUser != null)
                {
                    return Conflict(new { message = "User already exists" });
                }

                var user = new User
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,

                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var token = GenerateToken(user);
                var response = new
                {
                    message = "User SignUp Successfully",
                    token = token,
                    user = new
                    {
                        email = user.Email,
                        userName = user.UserName,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        profilePicture = user.ProfilePicture,
                        intersts = user.Intersts.Select(i => i.interst).ToList(),
                        follow = user.Follow.Select(f => f.UserId2).ToList(),
                        followed = user.Followed.Select(f => f.UserId1).ToList(),
                        posts = user.Posts.Select(p => new
                        {
                            id = p.Id,
                            content = p.Content,
                            date = p.CreatedAt,
                            likes = p.PostLikes.Count,
                            comments = p.PostComments.Count,
                        }).ToList()
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    message = ex.Message,
                };
                return BadRequest(response);
            }
        }


        [HttpPost("UploadProfilePicture")]
        public async Task<IActionResult> ProfilePic(IFormFile picture)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return BadRequest(new { message = "User Not Exists" });
            if (user.ProfilePicture != null)
            {
                user.ProfilePicture = null;
                await _context.SaveChangesAsync();
            }

            try
            {
                using (var stream = new MemoryStream())
                {
                    await picture.CopyToAsync(stream);
                    user.ProfilePicture = stream.ToArray();
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        profilePic = user.ProfilePicture
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }

        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn([FromBody] LogInDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data", details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }
            try
            {
                var user = await _context.Users
                    .Include(u => u.Intersts)
                    .Include(u => u.Follow)
                    .Include(u => u.Followed)
                    .Include(u => u.Posts)
                    .Include(u => u.Comments)
                    .Include(u => u.Likes)
                    .Include(u => u.Trips)
                    .FirstOrDefaultAsync(x => x.Email == dto.email);
                if (user == null || !Verify(dto.password, user.Password))
                {
                    return BadRequest(new { message = "The Email or Password Is Invalid" });
                }
                var token = GenerateToken(user);
                var response = new
                {
                    message = "User Logged In Successfully",
                    token = token,
                    user = new
                    {
                        email = user.Email,
                        userName = user.UserName,
                        firstName = user.FirstName,
                        lastName = user.LastName,
                        profilePicture = user.ProfilePicture,
                        intersts = user.Intersts.Select(i => i.interst).ToList(),
                        follow = user.Follow.Select(f => f.UserId2).ToList(),
                        followed = user.Followed.Select(f => f.UserId1).ToList(),
                        posts = user.Posts.Select(p => new
                        {
                            id = p.Id,
                            content = p.Content,
                            date = p.CreatedAt,
                            likes = p.PostLikes.Count,
                            comments = p.PostComments.Count,
                        }).ToList()
                    }

                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    message = ex.Message,
                };
                return BadRequest(response);
            }
        }

        //Add Interests
        [HttpPost("AddIntersts")]
        [Authorize]
        public async Task<IActionResult> AddIntersts([FromBody] InterstsDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            Console.WriteLine(userIdClaim);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                List<String> intersts = dto.intersts;
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (user == null)
                    return NotFound();
                foreach (var intrst in dto.intersts)
                {
                    user.Intersts.Add(new Intersts { userId = userId, interst = intrst });
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                var response = new
                {
                    message = ex.InnerException,
                };
                return BadRequest(response);
            }
        }


        //Update User
        [HttpPatch("UpdateUser")]
        [Authorize]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto dto, IFormFile? picture = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    message = "Invalid data",
                    details = ModelState
                    .Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            try
            {
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized(new { message = "User ID not found in token" });
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return BadRequest(new { message = "Invalid user ID in token" });
                }


                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                if (!string.IsNullOrEmpty(dto.UserName))
                {
                    user.UserName = dto.UserName;
                }
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    var existUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
                    if (existUser == null)
                        user.Email = dto.Email;
                    else
                        return BadRequest("User Email Exist");
                }
                if (!string.IsNullOrEmpty(dto.Password))
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password); // Hash the password
                }
                if (picture != null)
                {
                    using (var stream = new MemoryStream())
                    {
                        await picture.CopyToAsync(stream);
                        user.ProfilePicture = stream.ToArray();
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                var response = new
                {
                    message = ex.InnerException,
                };
                return BadRequest(response);
            }
        }

        [HttpGet("ValidToken")]
        [Authorize]
        public Task<ActionResult<bool>> ValidToken()
        {
            return Task.FromResult<ActionResult<bool>>(Ok(true));
        }



        //Tools
        private string GenerateToken(User user)
        {
            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
            claims.Add(new Claim(ClaimTypes.Name, user.UserName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:SecurityKey"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                issuer: _configuration["JWT:Issuer"],
                expires: DateTime.Now.AddDays(7),
                signingCredentials: credentials
                );
            var _token = new JwtSecurityTokenHandler().WriteToken(token);
            return _token;
        }
        private bool Verify(string password, string hashedpassword)
        {
            return (BCrypt.Net.BCrypt.Verify(password, hashedpassword));
        }
    }
}
