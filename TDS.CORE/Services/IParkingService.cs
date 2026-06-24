using TDS.MODELS.DTOs;

namespace TDS.CORE.Services;

public interface IParkingService
{
    Task<ParkVehicleResponse> ParkVehicleAsync(ParkVehicleRequest request);
    Task<ParkingStatusResponse> GetParkingStatusAsync();
    Task<ExitVehicleResponse> ExitVehicleAsync(ExitVehicleRequest request);
}
