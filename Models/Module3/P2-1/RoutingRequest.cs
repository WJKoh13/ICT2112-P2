using ProRental.Domain.Enums;

namespace ProRental.Models.Module3.P2_1;

/// <summary>
/// Temporary routing DTO for the prototype adapter. It captures the subset of inputs
/// Feature 1 currently forwards while the full Feature 3 request model is still evolving.
/// by: ernest
/// </summary>
public sealed record RoutingRequest(
    int OrderId,
    string DestinationAddress,
    double WeightKg,
    int Quantity,
    PreferenceType PreferenceType);
