using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using CsvHelper.Configuration;
using System.Formats.Asn1;

public class ImportPosts
{
    private readonly ApplicationDbContext _context;
    private readonly string _filePath;

    public ImportPosts(ApplicationDbContext context, string filePath)
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

        List<Post> posts = new List<Post>();

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
            var records = csv.GetRecords<PostCsv>().ToList();

            foreach (var record in records)
            {
                if (!int.TryParse(record.UserId, out int userId))
                {
                    Console.WriteLine($"⚠ تخطي سطر بسبب UserId غير صالح: {record.UserId}");
                    continue; // تجاهل الصف الذي يحتوي على UserId غير صالح
                }

                posts.Add(new Post
                {
                    Content = record.Content.Trim(),
                    Category = record.Category.Trim(),
                    UserId = userId,
                    CreatedAt = DateTime.Now
                });
            }

            if (posts.Any())
            {
                await _context.Posts.AddRangeAsync(posts);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✅ تم استيراد {posts.Count} بوست بنجاح!");
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

// تعريف الكلاس المطابق لملف CSV
public class PostCsv
{
    public string Content { get; set; }
    public string Category { get; set; }
    public string UserId { get; set; }
}
