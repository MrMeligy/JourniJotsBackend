using Backend.Models;
using CsvHelper.Configuration;
using CsvHelper;
using System.Globalization;

namespace Backend.Data
{
    public class ImportActivities
    {
        private readonly ApplicationDbContext _context;
        private readonly string _filePath;

        public ImportActivities(ApplicationDbContext context, string filePath)
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

            List<Activity> activities = new List<Activity>();

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
                var records = csv.GetRecords<WTD_CSV>().ToList();

                foreach (var record in records)
                {
                    activities.Add(new Activity
                    {
                        Name = record.Name.Trim(),
                        Address = record.Address.Trim(),
                        City = record.City.Trim(),
                        Latitude = record.Latitude,
                        Longitude = record.Longitude,
                        Rating = record.Rating,
                        RatingCount = record.RatingCount,
                        Image = record.Image.Trim()
                    });
                }

                if (activities.Any())
                {
                    await _context.Activities.AddRangeAsync(activities);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"IMPORT {activities.Count} Activity !");
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

public class WTD_CSV
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public float Rating { get; set; }
    public int RatingCount { get; set; }
    public string Image { get; set; }
    public string City { get; set; }

}
