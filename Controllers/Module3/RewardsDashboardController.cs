// Controllers/Module3/RewardsDashboardController.cs
// Boundary (ECB) — handles HTTP for the Rewards Dashboard feature (Feature 4).
// Depends only on IRewardsControl; never on RewardsControl directly.

using Microsoft.AspNetCore.Mvc;
using ProRental.Interfaces.Domain;
using ProRental.Domain.Entities;

namespace ProRental.Controllers;

public class RewardsDashboardController : Controller
{
    private readonly IRewardsControl _rewardsControl;

    public RewardsDashboardController(IRewardsControl rewardsControl)
    {
        _rewardsControl = rewardsControl;
    }

    // ── GET /RewardsDashboard ─────────────────────────────────────────────────

    /// <summary>Displays the rewards dashboard with all order carbon data.</summary>
    public IActionResult DisplayRewards()
    {
        var carbonData = _rewardsControl.GetAllOrderCarbonData();
        return View("~/Views/Module3/RewardsDashboard/Index.cshtml", carbonData);
    }

    // ── GET /RewardsDashboard/EcoScore?orderId=X ──────────────────────────────

    /// <summary>Returns the eco-score for a specific order as a partial view.</summary>
    [HttpGet]
    public IActionResult CalculateEcoScore(int orderId)
    {
        var score = _rewardsControl.CalculateEcoScore(orderId);

        if (score < 0)
        {
            TempData["Error"] = $"No carbon data found for Order #{orderId}.";
            return RedirectToAction(nameof(DisplayRewards));
        }

        ViewBag.OrderId  = orderId;
        ViewBag.EcoScore = score;
        return View("~/Views/Module3/RewardsDashboard/EcoScore.cshtml");
    }

    // ── POST /RewardsDashboard/DetermineReward ────────────────────────────────

    /// <summary>
    /// Determines and saves a reward for the customer based on the given order's
    /// eco-score, then redirects back to the dashboard.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DetermineReward(int orderId, int customerId)
    {
        var reward = _rewardsControl.DetermineReward(orderId, customerId);

        TempData["RewardMessage"] = reward is null
            ? $"Order #{orderId} does not qualify for a reward (carbon footprint too high)."
            : $"Reward applied: {reward.GetFormattedRewardValue()}";

        return RedirectToAction(nameof(DisplayRewards));
    }

    // ── POST /RewardsDashboard/RecordCarbonData ───────────────────────────────

    /// <summary>Records carbon data for an order and redirects to the dashboard.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecordCarbonData(int orderId, double totalCarbon)
    {
        _rewardsControl.CreateOrderCarbonData(orderId, totalCarbon);
        TempData["SuccessMessage"] = $"Carbon data recorded for Order #{orderId}.";
        return RedirectToAction(nameof(DisplayRewards));
    }
}
