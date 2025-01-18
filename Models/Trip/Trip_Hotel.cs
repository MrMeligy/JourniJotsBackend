namespace Backend.Models
{
    public class Trip_Hotel
    {
        public int TripId { get; set; }
        public int HotelId { get; set; }
        public Trip Trip { get; set; }
        public Hotel Hotel { get; set; }

    }
}
