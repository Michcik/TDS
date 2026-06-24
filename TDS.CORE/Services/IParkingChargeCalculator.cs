using TDS.MODELS;

namespace TDS.CORE.Services;

public interface IParkingChargeCalculator
{
    double Calculate(VehicleType vehicleType, DateTime timeIn, DateTime timeOut);
}
