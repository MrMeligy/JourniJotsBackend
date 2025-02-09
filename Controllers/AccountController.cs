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
                    City = dto.City,
                    DateOfBirth = dto.DateOfBirth,
                    FavFood = dto.Fav_Food
                };

                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                var token = GenerateToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == dto.email);
                if (user == null || !Verify(dto.password, user.Password))
                {
                    return BadRequest("The Email or Password Is Invalid");
                }
                var token = GenerateToken(user);
                return Ok(token);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateUser")]
        [Authorize]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UpdateUserDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Invalid data", details = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
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


                var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
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
                if (!string.IsNullOrEmpty(dto.City))
                {
                    user.City = dto.City;
                }
                if (!string.IsNullOrEmpty(dto.Fav_Food))
                {
                    user.FavFood = dto.Fav_Food;
                }
                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ValidToken")]
        [Authorize]
        public async Task<ActionResult<bool>> ValidToken()
        {
            return Ok(true);
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
