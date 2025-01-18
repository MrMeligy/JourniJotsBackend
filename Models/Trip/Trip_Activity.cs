namespace Backend.Models
{
    public class Trip_Activity
    {
        public int TripId { get; set; }
        public int ActivityId { get; set; }
        public Trip Trip { get; set; }
        public Activity Activity { get; set; }
    }
}
