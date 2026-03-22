// Domain/Module3/P2-5/ProductFootprintServiceStub.cs
// Temporary stub for IProductFootprintService.
// Allows Feature 4 to compile and run independently of Feature 1.
// DELETE this file and update the DI registration once Feature 1 is merged.

using ProRental.Interfaces.Domain;

namespace ProRental.Domain.Controls;

/// <summary>
/// No-op stub. Always returns 0 for product carbon footprint.
/// Replace with Feature 1's real implementation after integration.
/// </summary>
internal sealed class ProductFootprintServiceStub : IProductFootprintService
{
    public double CalculateProductFootprint(int orderId) => 0.0;
}
