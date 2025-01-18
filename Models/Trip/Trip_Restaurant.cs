namespace Backend.Models
{
    public class Trip_Restaurant
    {
        public int TripId { get; set; }
        public int RestaurantId { get; set; }
        public Trip Trip { get; set; }
        public Restaurant Restaurant { get; set; }
    }
}
