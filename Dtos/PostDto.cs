namespace Backend.Dtos
{
    public class PostDto
    {
        public string UserName { get; set; }
        public byte[]? ProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PostId { get; set; }
        public string Content { get; set; }
        public string Category { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; }
        public List<PostImageDto> PostImages { get; set; }
    }
    public class PostImageDto
    {
        public int Id { get; set; }
        public string ImageData { get; set; }
    }
}
