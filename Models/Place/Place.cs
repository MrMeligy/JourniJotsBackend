namespace Backend.Models
{
    public abstract class Place
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public int Rating { get; set; }
        public int RatingCount { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
