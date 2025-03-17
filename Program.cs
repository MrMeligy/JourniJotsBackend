
using Backend.Extensions;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Backend
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var ConnectionString = builder.Configuration.GetConnectionString("CS"); //ConnectionString in my local device
            builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(ConnectionString)); //Inject DataBase

            //use cors for any call out of the project
            builder.Services.AddCors();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGenJwtAuth();

            builder.Services.AddCustomJwtAuthExtension(builder.Configuration); //My Extension

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            //Allow any Call from any one
            app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            /*using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();

                string filePath = "E:/Graduation Project/Models/egypt_tourism_posts_updated.csv";

                var importer = new ImportPosts(context, filePath);
                await importer.ImportAsync();
            }*/

            app.Run();
            return Task.CompletedTask;
        }
    }
}
