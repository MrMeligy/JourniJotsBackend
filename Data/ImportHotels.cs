using Backend.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Backend.Data
{
    public class ImportHotels
    {
        private readonly ApplicationDbContext _context;
        private readonly string _filePath;

        public ImportHotels(ApplicationDbContext context, string filePath)
        {
            _context = context;
            _filePath = filePath;
        }

        public async Task ImportAsync()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"file doesn't exist {_filePath}");
                return;
            }

            List<Hotel> hotels = new List<Hotel>();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                MissingFieldFound = null,  // تجاهل الحقول المفقودة
                BadDataFound = null        // تجاهل البيانات الفاسدة
            };

            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, config);
                var records = csv.GetRecords<Hotel_CSV>().ToList();

                foreach (var record in records)
                {
                    hotels.Add(new Hotel
                    {
                        Name = record.Name.Trim(),
                        Address = record.Address.Trim(),
                        City = record.City.Trim(),
                        Latitude = record.Latitude,
                        Longitude = record.Longitude,
                        Rating = record.rating,
                        RatingCount = record.rating_count,
                        Image = record.Image.Trim()
                    });
                }

                if (hotels.Any())
                {
                    await _context.Hotels.AddRangeAsync(hotels);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"IMPORT {hotels.Count} Hotel !");
                }
                else
                {
                    Console.WriteLine("No hotels to import.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error importing hotels: {ex.Message}");
            }
        }

    }
}

public class Hotel_CSV
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public float Longitude { get; set; }
    public float Latitude { get; set; }
    public float rating { get; set; }
    public int rating_count { get; set; }
    public string Image { get; set; }
    public string City { get; set; }
}
