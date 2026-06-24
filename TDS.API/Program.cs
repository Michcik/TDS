
using Microsoft.EntityFrameworkCore;
using TDS.CORE.Services;
using TDS.DATA;

namespace TDS.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "TDS API", Version = "v1" });
            });

            // In-memory database
            builder.Services.AddDbContext<CarParkDbContext>(options =>
                options.UseInMemoryDatabase("CarParkDb"));

            // Application services
            builder.Services.AddScoped<IParkingService, ParkingService>();
            builder.Services.AddSingleton<IParkingChargeCalculator, ParkingChargeCalculator>();

            var app = builder.Build();

            // Seed the database on startup
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<CarParkDbContext>();
                db.Database.EnsureCreated();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();

        }
    }
}
