using Microsoft.AspNetCore.Mvc;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Controllers;

/// <summary>
/// HTTP boundary for Feature 1. It coordinates the shipping-option flow and keeps
/// ranking, persistence, routing, and carbon logic behind service contracts.
/// by: ernest
/// </summary>
public sealed class ShippingOptionsController : Controller
{
    private const string ViewRoot = "~/Views/Module3/P2-1/ShippingOptions/";
    private readonly IShippingOptionService _shippingOptionService;
    private readonly IRankingService _rankingService;

    public ShippingOptionsController(IShippingOptionService shippingOptionService, IRankingService rankingService)
    {
        _shippingOptionService = shippingOptionService;
        _rankingService = rankingService;
    }

    [HttpGet]
    public async Task<IActionResult> GetShippingOptions(int orderId, CancellationToken cancellationToken)
    {
        var options = await _shippingOptionService.GetShippingOptionsForOrderAsync(orderId, cancellationToken);
        ViewData["OrderId"] = orderId;
        return View($"{ViewRoot}Index.cshtml", options);
    }

    [HttpGet]
    public async Task<IActionResult> CompareOptions(int orderId, CancellationToken cancellationToken)
    {
        var options = await _shippingOptionService.GetShippingOptionsForOrderAsync(orderId, cancellationToken);
        ViewData["OrderId"] = orderId;
        // These ranked lists are prepared here so the compare page can show the same
        // option set through each ranking criterion without embedding ranking logic in Razor.
        ViewData["SpeedRanked"] = _rankingService.RankBySpeed(options);
        ViewData["CostRanked"] = _rankingService.RankByCost(options);
        ViewData["CarbonRanked"] = _rankingService.RankByCarbon(options);

        return View($"{ViewRoot}Compare.cshtml", options);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectShippingOption(int orderId, int optionId, CancellationToken cancellationToken)
    {
        var result = await _shippingOptionService.ApplyCustomerSelectionAsync(
            new SelectShippingOptionRequest(orderId, optionId),
            cancellationToken);

        return View($"{ViewRoot}Selected.cshtml", result);
    }
}
