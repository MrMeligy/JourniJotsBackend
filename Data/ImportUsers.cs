using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

public class ImportUsers
{
    private readonly ApplicationDbContext _context;
    private readonly string _filePath;

    public ImportUsers(ApplicationDbContext context, string filePath)
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

        List<User> users = new List<User>();

        using (StreamReader reader = new StreamReader(_filePath))
        {
            // قراءة العناوين وتجاهلها
            string headerLine = reader.ReadLine();

            while (!reader.EndOfStream)
            {
                string? line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');

                try
                {
                    // التأكد من عدم وجود المستخدم مسبقًا
                    if (await _context.Users.AnyAsync(u => u.Email == values[0].Trim()))
                    {
                        Console.WriteLine($"user {values[0]} is already exist.");
                        continue;
                    }

                    User user = new User
                    {
                        Email = values[0].Trim(),
                        Password = BCrypt.Net.BCrypt.HashPassword(values[1].Trim()), // 🔐 تشفير الباسوورد
                        FirstName = values[2].Trim(),
                        LastName = values[3].Trim(),
                        UserName = values[4].Trim(),

                    };

                    users.Add(user);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"error {values[0]}: {ex.Message}");
                }
            }
        }

        // إدخال البيانات في قاعدة البيانات
        if (users.Any())
        {
            await _context.Users.AddRangeAsync(users);
            await _context.SaveChangesAsync();
            Console.WriteLine("users import successfuly");
        }
        else
        {
            Console.WriteLine("No Data");
        }
    }
}
