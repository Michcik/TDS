
using Microsoft.AspNetCore.Mvc;
using TDS.CORE.Services;
using TDS.MODELS.DTOs;

namespace TDS.API.Controllers;

[ApiController]
[Route("parking")]
public class ParkingController : ControllerBase
{
    private readonly IParkingService _parkingService;
    private readonly ILogger<ParkingController> _logger;

    public ParkingController(IParkingService parkingService, ILogger<ParkingController> logger)
    {
        _parkingService = parkingService;
        _logger = logger;
    }

    /// <summary>
    /// Parks a vehicle in the first available space.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ParkVehicleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ParkVehicle([FromBody] ParkVehicleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VehicleReg))
            return BadRequest(ProblemDetailsFor("VehicleReg is required."));

        try
        {
            var result = await _parkingService.ParkVehicleAsync(request);
            return CreatedAtAction(nameof(ParkVehicle), result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid vehicle type provided.");
            return BadRequest(ProblemDetailsFor(ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("already parked"))
        {
            _logger.LogWarning(ex, "Vehicle already parked.");
            return Conflict(ProblemDetailsFor(ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No available"))
        {
            _logger.LogWarning(ex, "No spaces available.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ProblemDetailsFor(ex.Message));
        }
    }

    /// <summary>
    /// Returns the number of available and occupied parking spaces.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ParkingStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetParkingStatus()
    {
        var result = await _parkingService.GetParkingStatusAsync();
        return Ok(result);
    }

    /// <summary>
    /// Processes a vehicle exit: calculates charge and frees the space.
    /// </summary>
    [HttpPost("exit")]
    [ProducesResponseType(typeof(ExitVehicleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExitVehicle([FromBody] ExitVehicleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.VehicleReg))
            return BadRequest(ProblemDetailsFor("VehicleReg is required."));

        try
        {
            var result = await _parkingService.ExitVehicleAsync(request);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Vehicle not found in car park.");
            return NotFound(ProblemDetailsFor(ex.Message));
        }
    }

    private static ProblemDetails ProblemDetailsFor(string detail) => new ProblemDetails() { Detail = detail };
}
