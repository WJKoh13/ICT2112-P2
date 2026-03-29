using Microsoft.AspNetCore.Mvc;
using ProRental.Domain.Controls;
using ProRental.Domain.Enums;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Controllers;

/// <summary>
/// HTTP boundary for Feature 1. It coordinates the shipping-option flow and keeps
/// persistence, routing, and carbon logic behind service contracts.
/// by: ernest
/// </summary>
public sealed class ShippingOptionsController : Controller
{
    private const string ViewRoot = "~/Views/Module3/P2-1/ShippingOptions/";
    private readonly IShippingOptionService _shippingOptionService;

    public ShippingOptionsController(IShippingOptionService shippingOptionService)
    {
        _shippingOptionService = shippingOptionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetShippingOptions(int checkoutId, CancellationToken cancellationToken)
    {
        var options = await _shippingOptionService.GetPreferenceChoicesForCheckoutAsync(checkoutId, cancellationToken);
        ViewData["CheckoutId"] = checkoutId;
        return View($"{ViewRoot}Index.cshtml", options);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectShippingPreference(int checkoutId, PreferenceType preferenceType, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _shippingOptionService.ConfirmPreferenceSelectionAsync(
                new SelectShippingPreferenceRequest(checkoutId, preferenceType),
                cancellationToken);

            return View($"{ViewRoot}Selected.cshtml", result);
        }
        catch (RouteResolutionException exception)
        {
            ModelState.AddModelError(string.Empty, exception.Message);
            var options = await _shippingOptionService.GetPreferenceChoicesForCheckoutAsync(checkoutId, cancellationToken);
            ViewData["CheckoutId"] = checkoutId;
            return View($"{ViewRoot}Index.cshtml", options);
        }
    }
}
