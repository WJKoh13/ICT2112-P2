using Microsoft.AspNetCore.Mvc;
using ProRental.Interfaces.Domain;

namespace ProRental.Controllers;

public class RewardsDashboardController : Controller
{
    private readonly IRewardsControl _rewardsControl;

    public RewardsDashboardController(IRewardsControl rewardsControl)
    {
        _rewardsControl = rewardsControl;
    }

    // ── DisplayRewards ────────────────────────────────────────────────────────
    // Main dashboard — shows all orders with carbon data + rewards history.

    public IActionResult DisplayRewards()
    {
        ViewBag.CarbonRecords = _rewardsControl.GetAllCarbonRecords();
        ViewBag.Rewards       = _rewardsControl.GetAllRewards();
        return View("~/Views/Module3/RewardsDashboard/Index.cshtml");
    }

    // ── CalculateEcoScore ─────────────────────────────────────────────────────
    // Called from the dashboard table — calculates score for one order.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CalculateEcoScore(int orderId)
    {
        int score = _rewardsControl.CalculateEcoScore(orderId);

        TempData["ScoreMessage"] = score < 0
            ? $"No carbon data found for Order #{orderId}. Record carbon data first."
            : $"Order #{orderId} — Eco Score: {score}/100";

        return RedirectToAction(nameof(DisplayRewards));
    }

    // ── DetermineReward ───────────────────────────────────────────────────────
    // Called from the dashboard table — issues reward if eligible.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DetermineReward(int orderId)
    {
        var reward = _rewardsControl.DetermineReward(orderId);

        TempData["RewardMessage"] = reward is not null
            ? $"Reward issued for Order #{orderId}: {reward.GetFormattedValue()}"
            : $"Order #{orderId} does not qualify for a reward (impact too high).";

        return RedirectToAction(nameof(DisplayRewards));
    }

    // ── RecordCarbonData ──────────────────────────────────────────────────────
    // Form submission — records carbon data for a new order.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecordCarbonData(int orderId, double totalCarbon)
    {
        _rewardsControl.CreateOrderCarbonData(orderId, totalCarbon);
        TempData["SuccessMessage"] = $"Carbon data recorded for Order #{orderId}.";
        return RedirectToAction(nameof(DisplayRewards));
    }
}