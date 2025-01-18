namespace Backend.Models
{
    public class Trip
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public ICollection<Trip_Activity> Activities { get; set; } = new List<Trip_Activity>();
        public ICollection<Trip_Restaurant> Restaurants { get; set; } = new List<Trip_Restaurant>();
        public ICollection<Trip_Hotel> Hotels { get; set; } = new List<Trip_Hotel>();
    }
}
