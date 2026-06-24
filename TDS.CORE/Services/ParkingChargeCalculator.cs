using TDS.MODELS;

namespace TDS.CORE.Services;

public class ParkingChargeCalculator : IParkingChargeCalculator
{
    private const double SurchargePerFiveMinutes = 1.00;

    private readonly Dictionary<VehicleType, double> _ratesPerMinute = new()
    {
        { VehicleType.SmallCar,  0.10 },
        { VehicleType.MediumCar, 0.20 },
        { VehicleType.LargeCar,  0.40 }
    };

    public double Calculate(VehicleType vehicleType, DateTime timeIn, DateTime timeOut)
    {
        var totalMinutes = (timeOut - timeIn).TotalMinutes;
        var baseCharge = _ratesPerMinute[vehicleType] * totalMinutes;
        var surchargeBlocks = Math.Floor(totalMinutes / 5);
        var surcharge = surchargeBlocks * SurchargePerFiveMinutes;
        var total = baseCharge + surcharge;

        return Math.Round(total, 2);
    }
}
