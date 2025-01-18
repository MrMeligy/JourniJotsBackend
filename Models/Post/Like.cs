namespace Backend.Models
{
    public class Like
    {
        public int PostId { get; set; }
        public int UserId { get; set; }

        public Post Post { get; set; }
        public User User { get; set; }

    }
}
