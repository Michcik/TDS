using FluentAssertions;
using TDS.CORE.Services;
using TDS.MODELS;

namespace TDS.TESTS;

public class ParkingChargeCalculatorTests
{
    private readonly ParkingChargeCalculator _calculator = new();

    [Theory]
    [InlineData(VehicleType.SmallCar,  1, 0.10)]   
    [InlineData(VehicleType.MediumCar, 1, 0.20)]   
    [InlineData(VehicleType.LargeCar,  1, 0.40)]   
    [InlineData(VehicleType.SmallCar,  4, 0.40)]  
    public void Calculate_BasedOnRateAndMinutes_NoSurcharge(VehicleType vehicleType, int minutes, double expectedCharge)
    {
        // Arrange
        var timeIn  = new DateTime(2024, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var timeOut = timeIn.AddMinutes(minutes);
        // Act
        var result = _calculator.Calculate(vehicleType, timeIn, timeOut);
        // Assert
        result.Should().BeApproximately(expectedCharge, 0.001);
    }

    [Fact]
    public void Calculate_SmallCar_FiveMinutes_IncludesSurcharge()
    {
        // Arrange
        var timeIn  = new DateTime(2024, 1, 1, 9, 0, 0, DateTimeKind.Utc);
        var timeOut = timeIn.AddMinutes(5);
        // Act
        var result = _calculator.Calculate(VehicleType.SmallCar, timeIn, timeOut);
        // Assert
        result.Should().BeApproximately(1.50, 0.001);
    }
}
