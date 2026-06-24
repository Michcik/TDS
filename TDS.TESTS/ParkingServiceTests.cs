using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TDS.CORE.Services;
using TDS.DATA;
using TDS.MODELS;
using TDS.MODELS.DTOs;

namespace TDS.TESTS;

public class ParkingServiceTests
{
    private static CarParkDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<CarParkDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        var context = new CarParkDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static IParkingChargeCalculator CreateMockCalculator(double returnValue = 5.00)
    {
        var mock = new Mock<IParkingChargeCalculator>();
        mock.Setup(c => c.Calculate(It.IsAny<VehicleType>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .Returns(returnValue);
        return mock.Object;
    }

    [Fact]
    public async Task ParkVehicle_ValidRequest_ReturnsSpaceNumber()
    {
        using var context = CreateDbContext(nameof(ParkVehicle_ValidRequest_ReturnsSpaceNumber));
        var service = new ParkingService(context, CreateMockCalculator());

        var request = new ParkVehicleRequest("DWR 59231", 1);
        var result = await service.ParkVehicleAsync(request);

        result.VehicleReg.Should().Be("DWR 59231");
        result.SpaceNumber.Should().Be(1);
        result.TimeIn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task ParkVehicle_AllocatesFirstAvailableSpace()
    {
        using var context = CreateDbContext(nameof(ParkVehicle_AllocatesFirstAvailableSpace));
        var service = new ParkingService(context, CreateMockCalculator());

        await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG007", 1));

        var result = await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG008", 2));

        result.SpaceNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetParkingStatus_NoVehicles_ReturnsAllAvailable()
    {
        using var context = CreateDbContext(nameof(GetParkingStatus_NoVehicles_ReturnsAllAvailable));
        var service = new ParkingService(context, CreateMockCalculator());

        var result = await service.GetParkingStatusAsync();

        result.AvailableSpaces.Should().Be(10);
        result.OccupiedSpaces.Should().Be(0);
    }

    [Fact]
    public async Task GetParkingStatus_AfterParking_ReflectsOccupancy()
    {
        using var context = CreateDbContext(nameof(GetParkingStatus_AfterParking_ReflectsOccupancy));
        var service = new ParkingService(context, CreateMockCalculator());

        await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG007", 1));
        await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG008", 2));

        var result = await service.GetParkingStatusAsync();

        result.AvailableSpaces.Should().Be(8);
        result.OccupiedSpaces.Should().Be(2);
    }

    [Fact]
    public async Task ExitVehicle_ValidVehicle_FreesSpaceAndReturnsCharge()
    {
        using var context = CreateDbContext(nameof(ExitVehicle_ValidVehicle_FreesSpaceAndReturnsCharge));
        var service = new ParkingService(context, CreateMockCalculator(returnValue: 3.50));

        await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG007", 1));
        var result = await service.ExitVehicleAsync(new ExitVehicleRequest("DWRG007"));

        result.VehicleReg.Should().Be("DWRG007");
        result.VehicleCharge.Should().Be(3.50);
        result.TimeOut.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Space should be freed
        var status = await service.GetParkingStatusAsync();
        status.AvailableSpaces.Should().Be(10);
    }

    [Fact]
    public async Task ExitVehicle_AfterExit_SpaceCanBeReallocated()
    {
        using var context = CreateDbContext(nameof(ExitVehicle_AfterExit_SpaceCanBeReallocated));
        var service = new ParkingService(context, CreateMockCalculator());

        await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG007", 1));
        await service.ExitVehicleAsync(new ExitVehicleRequest("DWRG007"));

        // A new vehicle should be able to park in space 1 again
        var result = await service.ParkVehicleAsync(new ParkVehicleRequest("DWRG008", 1));
        result.SpaceNumber.Should().Be(1);
    }
}
