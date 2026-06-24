using Microsoft.EntityFrameworkCore;
using TDS.MODELS;

namespace TDS.DATA;
public class CarParkDbContext : DbContext
{
    public CarParkDbContext(DbContextOptions<CarParkDbContext> options) : base(options) { }

    public DbSet<ParkingSpace> ParkingSpaces => Set<ParkingSpace>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Seed 10 parking spaces
        var spaces = Enumerable.Range(1, 10)
            .Select(i => new ParkingSpace
            {
                Id = i,
                SpaceNumber = i,
                IsOccupied = false,
                VehicleReg = null,
                VehicleType = null,
                TimeIn = null
            })
            .ToArray();

        modelBuilder.Entity<ParkingSpace>().HasData(spaces);
    }
}
