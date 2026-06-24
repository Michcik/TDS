using Microsoft.EntityFrameworkCore;
using TDS.DATA;
using TDS.MODELS;
using TDS.MODELS.DTOs;

namespace TDS.CORE.Services;

public class ParkingService : IParkingService
{
    private readonly CarParkDbContext _context;
    private readonly IParkingChargeCalculator _chargeCalculator;

    public ParkingService(CarParkDbContext context, IParkingChargeCalculator chargeCalculator)
    {
        _context = context;
        _chargeCalculator = chargeCalculator;
    }

    public async Task<ParkVehicleResponse> ParkVehicleAsync(ParkVehicleRequest request)
    {
        if (!Enum.IsDefined(typeof(VehicleType), request.VehicleType))
            throw new ArgumentException($"Invalid VehicleType '{request.VehicleType}'. Must be 1 (Small), 2 (Medium), or 3 (Large).");

        // Check if vehicle is already parked
        var alreadyParked = await _context.ParkingSpaces
            .AnyAsync(s => s.VehicleReg == request.VehicleReg && s.IsOccupied);

        if (alreadyParked)
            throw new InvalidOperationException($"Vehicle '{request.VehicleReg}' is already parked.");

        // Find the first available space (lowest space number)
        var availableSpace = await _context.ParkingSpaces
            .Where(s => !s.IsOccupied)
            .OrderBy(s => s.SpaceNumber)
            .FirstOrDefaultAsync();

        if (availableSpace is null)
            throw new InvalidOperationException("No available parking spaces.");

        var timeIn = DateTime.UtcNow;

        availableSpace.IsOccupied = true;
        availableSpace.VehicleReg = request.VehicleReg;
        availableSpace.VehicleType = (VehicleType)request.VehicleType;
        availableSpace.TimeIn = timeIn;

        await _context.SaveChangesAsync();

        return new ParkVehicleResponse(request.VehicleReg, availableSpace.SpaceNumber, timeIn);
    }

    public async Task<ParkingStatusResponse> GetParkingStatusAsync()
    {
        var spaces = await _context.ParkingSpaces.ToListAsync();
        var available = spaces.Count(s => !s.IsOccupied);
        var occupied = spaces.Count(s => s.IsOccupied);

        return new ParkingStatusResponse(available, occupied);
    }

    public async Task<ExitVehicleResponse> ExitVehicleAsync(ExitVehicleRequest request)
    {
        var occupiedSpace = await _context.ParkingSpaces
            .FirstOrDefaultAsync(s => s.VehicleReg == request.VehicleReg && s.IsOccupied);

        if (occupiedSpace is null)
            throw new KeyNotFoundException($"Vehicle '{request.VehicleReg}' is not currently parked.");

        var timeOut = DateTime.UtcNow;
        var timeIn = occupiedSpace.TimeIn!.Value;
        var vehicleType = occupiedSpace.VehicleType!.Value;

        var charge = _chargeCalculator.Calculate(vehicleType, timeIn, timeOut);

        // Free up the space
        occupiedSpace.IsOccupied = false;
        occupiedSpace.VehicleReg = null;
        occupiedSpace.VehicleType = null;
        occupiedSpace.TimeIn = null;

        await _context.SaveChangesAsync();

        return new ExitVehicleResponse(request.VehicleReg, charge, timeIn, timeOut);
    }
}
