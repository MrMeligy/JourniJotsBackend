namespace Backend.Models
{
    public class Follow
    {
        public int UserId1 { get; set; } //follower
        public User User1 { get; set; }
        public int UserId2 { get; set; } //followed
        public User User2 { get; set; }
    }
}
