using Backend.Models;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace Backend.Data
{
    public class ImportRestaurants
    {
        private readonly ApplicationDbContext _context;
        private readonly string _filePath;

        public ImportRestaurants(ApplicationDbContext context, String filePath)
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

            List<Restaurant> restaurants = new List<Restaurant>();

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
                var records = csv.GetRecords<Restaurant_CSV>().ToList();

                foreach (var record in records)
                {
                    restaurants.Add(new Restaurant
                    {
                        Name = record.Name.Trim(),
                        Address = record.Address.Trim(),
                        City = record.city.Trim(),
                        Latitude = record.Longitude,
                        Longitude = record.Longitude,
                        Rating = record.rating,
                        RatingCount = record.ratingCount,
                        Image = record.Image.Trim(),
                        Category = record.Category.Trim(),
                    });
                }

                if (restaurants.Any())
                {
                    await _context.Restaurants.AddRangeAsync(restaurants);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"IMPORT {restaurants.Count} Restaurant !");
                }
                else
                {
                    Console.WriteLine("⚠ لم يتم استيراد أي بيانات صالحة.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ خطأ أثناء استيراد البيانات: {ex.Message}");
            }
        }

    }
}

public class Restaurant_CSV
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public float Longitude { get; set; }
    public float rating { get; set; }
    public int ratingCount { get; set; }
    public string Image { get; set; }
    public string city { get; set; }
    public string Category { get; set; }
}

