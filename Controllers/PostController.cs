using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Net.Mime.MediaTypeNames;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public PostController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("PostJot")]
        public async Task<IActionResult> PostJotAsync([FromBody] CreatePostDto dto)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound("User Not Exist");
            }
            try
            {
                var post = new Post
                {
                    UserId = userId,
                    Content = dto.Content,
                };
                await _context.Posts.AddAsync(post);
                _context.SaveChanges();
                var response = new
                {
                    userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    postId = post.Id,
                    Content = post.Content,
                    images = post.Images
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpPost("PostImage")]
        public async Task<IActionResult> PostImageAsync(int postId, List<IFormFile> images)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            try
            {
                var postImages = new List<PostImage>();
                if (images != null)
                {
                    foreach (var image in images)
                    {
                        using (var stream = new MemoryStream())
                        {
                            await image.CopyToAsync(stream);
                            var postImage = new PostImage
                            {
                                Image = stream.ToArray(),
                                PostId = postId,
                            };
                            postImages.Add(postImage);
                        }
                    }
                    await _context.PostImages.AddRangeAsync(postImages);
                    await _context.SaveChangesAsync();
                    return Ok(postImages);
                }
                return BadRequest("There is No Pictures");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }

        [HttpPost("ToggleLike")]
        public async Task<IActionResult> LikePostAsync(int postId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (post == null || user == null)
            {
                return NotFound();
            }
            try
            {
                var liked = await _context.PostLikes
                    .FirstOrDefaultAsync(p => p.PostId == postId && p.UserId == userId);
                if (liked != null)
                {
                    _context.PostLikes.Remove(liked);
                    await _context.SaveChangesAsync();
                    return Ok("Unliked");
                }
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId,
                };
                await _context.PostLikes.AddAsync(like);
                _context.SaveChanges();
                var response = new
                {
                    postId = postId,
                    UserName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpPost("Comment")]
        public async Task<IActionResult> CommentPostAsync(int postId, [FromBody] string content)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (post == null || user == null)
            {
                return NotFound();
            }
            try
            {
                var comment = new Comment
                {
                    PostId = postId,
                    UserId = userId,
                    Content = content,
                };
                await _context.PostComments.AddAsync(comment);
                await _context.SaveChangesAsync();
                var response = new
                {
                    postId = postId,
                    UserName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    content = content
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("GetUserPosts")]
        public async Task<IActionResult> GetUserPostsAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            try
            {
                var posts = await _context.Posts.Where(x => x.UserId == userId).Select(p => new
                {
                    UserName = p.User.UserName,
                    PostId = p.Id,
                    Post = p.Content,
                    LikeCount = p.PostLikes.Count,
                    CommentCount = p.PostComments.Count,
                    PostImages = p.Images.Select(pi => new
                    {
                        pi.Id,
                        ImageData = Convert.ToBase64String(pi.Image)
                    }).ToList()
                }).ToListAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("GetPosts")]
        public async Task<IActionResult> GetPostsAsync()
        {
            try
            {
                var posts = await _context.Posts.Select(p => new
                {
                    UserName = p.User.UserName,
                    PostId = p.Id,
                    Post = p.Content,
                    LikeCount = p.PostLikes.Count,
                    CommentCount = p.PostComments.Count,
                    PostImages = p.Images.Select(pi => new
                    {
                        pi.Id,
                        ImageData = Convert.ToBase64String(pi.Image)
                    }).ToList()
                }).ToListAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpGet("GetPostComments")]
        public async Task<IActionResult> GetPostCommentsAsync(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            try
            {
                var comments = await _context.PostComments.Where(x => x.PostId == postId).Select(c => new
                {
                    c.Id,
                    c.User.ProfilePicture,
                    c.UserId,
                    c.Content,
                    c.User.UserName,
                }).ToListAsync();
                return Ok(comments);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpDelete("DeletePost")]
        public async Task<IActionResult> DeletePostAsync(int postId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }

            var post = await _context.Posts.Include(p => p.PostComments).Include(p => p.PostLikes)
                .FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            if (post.UserId != userId)
            {
                return BadRequest("you can't Delete This Post");
            }
            try
            {
                if (post.PostComments.Any())
                {
                    _context.PostComments.RemoveRange(post.PostComments);
                }
                if (post.PostLikes.Any())
                {
                    _context.PostLikes.RemoveRange(post.PostLikes);
                }
                _context.Posts.Remove(post);
                await _context.SaveChangesAsync();
                return Ok("Post Deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }
        [HttpDelete("DeleteComment")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            var comment = await _context.PostComments.FirstOrDefaultAsync(c => c.Id == commentId);
            if (comment == null)
                return NotFound("Comment Not Foun");
            if (userId != comment.UserId)
                return BadRequest("You Can't Delete This Comment");
            try
            {
                _context.PostComments.Remove(comment);
                await _context.SaveChangesAsync();
                return Ok("Comment Deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.InnerException);
            }
        }

    }
}
