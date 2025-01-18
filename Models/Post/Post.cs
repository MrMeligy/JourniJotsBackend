namespace Backend.Models
{
    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; }
        public User User { get; set; }
        public ICollection<Like> PostLikes { get; set; } = new List<Like>();
        public ICollection<Comment> PostComments { get; set; } = new List<Comment>();
        public ICollection<PostImage> Images { get; set; } = new List<PostImage>();
    }
}
