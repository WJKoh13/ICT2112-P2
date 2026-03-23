using Microsoft.AspNetCore.Mvc;
using ProRental.Data.Module3.P2_5.Interfaces;
using ProRental.Domain.Entities;

namespace ProRental.Controllers.Module3.P2_5;

public class CarbonFootprintController : Controller
{
    private readonly IBuildingFootprintGateway _gateway;

    public CarbonFootprintController(IBuildingFootprintGateway gateway)
    {
        _gateway = gateway;
    }

    public IActionResult ProductFootprintView()
    {
        return View("~/Views/Module3/P2-5/ProductFootprintView.cshtml");
    }

    public IActionResult StaffFootprintView()
    {
        return View("~/Views/Module3/P2-5/StaffFootprintView.cshtml");
    }

    public IActionResult BuildingFootprintView()
    {
        return View("~/Views/Module3/P2-5/BuildingFootprintView.cshtml");
    }

    public IActionResult PackagingFootprintView()
    {
        return View("~/Views/Module3/P2-5/PackagingFootprintView.cshtml");
    }

    /// <summary>
    /// Calculate and create a building footprint record.
    /// Formula: totalRoomCo2 = Sr × Cr × Wz × Wf × k (where k = 0.000729)
    /// </summary>
    [HttpPost]
    [Route("api/building-footprint")]
    [ProducesResponseType(typeof(Buildingfootprint), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateBuildingFootprint([FromBody] BuildingFootprintRequest request)
    {
        // Validate roomSize and co2Level are positive
        if (request.RoomSize <= 0)
            return BadRequest(new { error = "roomSize must be a positive number." });

        if (request.Co2Level <= 0)
            return BadRequest(new { error = "co2Level must be a positive number." });

        var zoneWeights = new Dictionary<string, double>
        {
            { "North", 1.00 },
            { "South", 1.25 },
            { "East", 1.10 },
            { "West", 1.15 },
            { "Central", 1.35 }
        };

        var floorWeights = new Dictionary<string, double>
        {
            { "Level 1", 1.00 },
            { "Level 2", 1.20 },
            { "Level 3", 1.45 },
            { "Level 4", 1.60 },
            { "Level 5", 1.75 }
        };

        // Validate zone
        if (string.IsNullOrWhiteSpace(request.Zone) || !zoneWeights.ContainsKey(request.Zone))
            return BadRequest(new { error = "zone must be one of: North, South, East, West, Central." });

        // Validate floor
        if (string.IsNullOrWhiteSpace(request.Floor) || !floorWeights.ContainsKey(request.Floor))
            return BadRequest(new { error = "floor must be one of: Level 1, Level 2, Level 3, Level 4, Level 5." });

        // Validate block and room are not empty
        if (string.IsNullOrWhiteSpace(request.Block))
            return BadRequest(new { error = "block cannot be empty." });

        if (string.IsNullOrWhiteSpace(request.Room))
            return BadRequest(new { error = "room cannot be empty." });

        try
        {
            const double CalibrationConstant = 0.000729;

            // Calculate: Sr × Cr × Wz × Wf × k
            double totalRoomCo2 = request.RoomSize * request.Co2Level * zoneWeights[request.Zone] * floorWeights[request.Floor] * CalibrationConstant;
            totalRoomCo2 = Math.Round(totalRoomCo2, 2);

            // Create the footprint record
            var footprint = new Buildingfootprint
            {
                Timehourly = DateTime.UtcNow,
                Zone = request.Zone,
                Block = request.Block,
                Floor = request.Floor,
                Room = request.Room,
                Totalroomco2 = totalRoomCo2
            };

            // Save to database
            var created = await _gateway.CreateBuildingFootprintAsync(footprint);

            return CreatedAtAction(nameof(CalculateBuildingFootprint), created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"An error occurred: {ex.Message}" });
        }
    }
}

/// <summary>
/// Request model for building footprint calculation
/// </summary>
public class BuildingFootprintRequest
{
    public double RoomSize { get; set; }
    public double Co2Level { get; set; }
    public string Zone { get; set; } = string.Empty;
    public string Block { get; set; } = string.Empty;
    public string Floor { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
}
