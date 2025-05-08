namespace Backend.Models
{
    public class PostImage
    {
        public int Id { get; set; }
        public string Image { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
