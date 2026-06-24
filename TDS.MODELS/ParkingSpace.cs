namespace TDS.MODELS;

public class ParkingSpace
{
    public int Id { get; set; }
    public int SpaceNumber { get; set; }
    public bool IsOccupied { get; set; }
    public string? VehicleReg { get; set; }
    public VehicleType? VehicleType { get; set; }
    public DateTime? TimeIn { get; set; }
}
