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
                    CreatedAt = DateTime.Now,
                };
                await _context.Posts.AddAsync(post);
                _context.SaveChanges();
                var response = new
                {
                    userName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value,
                    postId = post.Id,
                    createdAt = post.CreatedAt,
                    Content = post.Content,
                    images = post.Images
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException });
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
                return BadRequest(new { message = ex.InnerException });
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
                    return Ok(false);
                }
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId,
                };
                await _context.PostLikes.AddAsync(like);
                _context.SaveChanges();

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException });
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
                    profilePicture = user.ProfilePicture,
                    content = content
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException });
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
                return BadRequest(new { message = ex.InnerException });
            }
        }
        [HttpGet("GetPosts")]
        public async Task<IActionResult> GetPostsAsync(
    [FromQuery] int pageSize = 10,
    [FromQuery] DateTime? lastPostDate = null) // استقبال آخر تاريخ بوست في الصفحة السابقة
        {
            pageSize = Math.Clamp(pageSize, 1, 100);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
            {
                return Unauthorized(new { message = "Invalid user authentication" });
            }

            try
            {
                // Fetch user interests
                var userInterests = await _context.Intersts
                    .Where(i => i.userId == parsedUserId)
                    .Select(i => i.interst)
                    .ToListAsync();

                // Base query for posts
                var postsQuery = _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Images)
                    .AsNoTracking()
                    .AsQueryable();

                // Filter by interests
                if (userInterests.Any())
                {
                    postsQuery = postsQuery.Where(p => userInterests.Contains(p.Category));
                }

                // Apply Keyset Pagination: تحميل البوستات الأحدث من آخر بوست في الصفحة السابقة
                if (lastPostDate.HasValue)
                {
                    postsQuery = postsQuery.Where(p => p.CreatedAt < lastPostDate.Value);
                }

                // ترتيب البوستات من الأحدث للأقدم
                postsQuery = postsQuery.OrderByDescending(p => p.CreatedAt);

                // جلب البيانات بحد أقصى `pageSize`
                var posts = await postsQuery
                    .Take(pageSize)
                    .Select(p => new PostDto
                    {
                        UserName = p.User.UserName,
                        ProfilePicture = p.User.ProfilePicture,
                        CreatedAt = p.CreatedAt,
                        PostId = p.Id,
                        Content = p.Content,
                        Category = p.Category,
                        LikeCount = _context.PostLikes.Count(l => l.PostId == p.Id),
                        CommentCount = _context.PostComments.Count(c => c.PostId == p.Id),
                        IsLikedByCurrentUser = _context.PostLikes.Any(l => l.PostId == p.Id && l.UserId == parsedUserId),
                        PostImages = p.Images.Select(pi => new PostImageDto
                        {
                            Id = pi.Id,
                            ImageData = Convert.ToBase64String(pi.Image)
                        }).ToList()
                    })
                    .ToListAsync();

                // الحصول على آخر تاريخ بوست لهذه الصفحة لاستخدامه في الصفحة التالية
                var newLastPostDate = posts.LastOrDefault()?.CreatedAt;

                return Ok(new
                {
                    Posts = posts,
                    LastPostDate = newLastPostDate // إرسال هذا التاريخ ليستخدم في الصفحة التالية
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving posts" });
            }
        }

        /*public async Task<IActionResult> GetPostsAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int parsedUserId))
            {
                return Unauthorized(new { message = "Invalid user ID in token" });
            }
            try
            {
                // Fetch user interests efficiently
                var userInterests = await _context.Intersts
                    .Where(i => i.userId == parsedUserId)
                    .Select(i => i.interst)
                    .ToListAsync();
                // Base query for posts with include to reduce database round trips
                var postsQuery = _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.PostLikes)
                    .Include(p => p.PostComments)
                    .Include(p => p.Images)
                    .AsQueryable();
                // Filter by interests if available
                if (userInterests.Any())
                {
                    postsQuery = postsQuery.Where(p => userInterests.Contains(p.Category));
                }
                // Project to DTO efficiently
                var posts = await postsQuery
                    .Select(p => new
                    {
                        UserName = p.User.UserName,
                        ProfilePicture = p.User.ProfilePicture,
                        CreatedAt = p.CreatedAt,
                        PostId = p.Id,
                        Post = p.Content,
                        LikeCount = p.PostLikes.Count,
                        CommentCount = p.PostComments.Count,
                        IsLikedByCurrentUser = p.PostLikes.Any(l => l.UserId == parsedUserId),
                        PostImages = p.Images.Select(pi => new
                        {
                            Id = pi.Id,
                            ImageData = Convert.ToBase64String(pi.Image)
                        }).ToList()
                    })
                    .AsSplitQuery() // Improve performance for complex queries
                    .AsNoTracking() // Disable change tracking for read-only queries
                    .ToListAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.InnerException });
            }
        }
*/
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
                return BadRequest(new { message = ex.InnerException });
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
