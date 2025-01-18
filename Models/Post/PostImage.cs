namespace Backend.Models
{
    public class PostImage
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
