namespace TDS.MODELS.DTOs;

public record ParkVehicleRequest(string VehicleReg, int VehicleType);

public record ParkVehicleResponse(string VehicleReg, int SpaceNumber, DateTime TimeIn);

public record ParkingStatusResponse(int AvailableSpaces, int OccupiedSpaces);

public record ExitVehicleRequest(string VehicleReg);

public record ExitVehicleResponse(string VehicleReg, double VehicleCharge, DateTime TimeIn, DateTime TimeOut);
