using Backend.Dtos;
using Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == dto.UserId);
            if (user == null)
            {
                return NotFound();
            }
            var post = new Post
            {
                UserId = dto.UserId,
                Content = dto.Content,
            };
            await _context.Posts.AddAsync(post);
            _context.SaveChanges();
            return Ok(post);
        }
        [HttpPost("PostImage")]
        public async Task<IActionResult> PostImageAsync(int postId, IFormFile image)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            using (var stream = new MemoryStream())
            {
                await image.CopyToAsync(stream);
                var postImage = new PostImage
                {
                    Image = stream.ToArray(),
                    PostId = postId,
                };
                await _context.PostImages.AddAsync(postImage);
                _context.SaveChanges();
                return Ok(postImage);
            }
        }
        [HttpPost("LikePost")]
        public async Task<IActionResult> LikePostAsync(int postId, int userId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (post == null || user == null)
            {
                return NotFound();
            }
            var like = new Like
            {
                PostId = postId,
                UserId = userId,
            };
            await _context.PostLikes.AddAsync(like);
            _context.SaveChanges();
            return Ok(like);
        }
        [HttpPost("CommentPost")]
        public async Task<IActionResult> CommentPostAsync(int postId, int userId, string content)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (post == null || user == null)
            {
                return NotFound();
            }
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
            };
            await _context.PostComments.AddAsync(comment);
            _context.SaveChanges();
            return Ok(comment);
        }
        [HttpGet("GetUserPosts")]
        public async Task<IActionResult> GetUserPostsAsync(int userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            var posts = await _context.Posts.Where(x => x.UserId == userId).Select(p => new
            {
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
        [HttpGet("GetPosts")]
        public async Task<IActionResult> GetPostsAsync()
        {
            var posts = await _context.Posts.Select(p => new
            {
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
        [HttpGet("GetPostComments")]
        public async Task<IActionResult> GetPostCommentsAsync(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            var comments = await _context.PostComments.Where(x => x.PostId == postId).Select(c => new
            {
                c.Content,
                c.User.UserName,
            }).ToListAsync();
            return Ok(comments);
        }
        [HttpDelete("DeletePost")]
        public async Task<IActionResult> DeletePostAsync(int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
            {
                return NotFound();
            }
            _context.Posts.Remove(post);
            _context.SaveChanges();
            return Ok(post);
        }


    }
}
