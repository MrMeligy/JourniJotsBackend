namespace Backend.Models
{
    public class Interests
    {
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public Activity Activity { get; set; }
    }
}
