using Microsoft.AspNetCore.Mvc;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Controllers;

public sealed class BatchAdminController : Controller
{
    private const string ViewRoot = "~/Views/Module3/P2-1/";

    private readonly IBatchDisplayManager _batchDisplayManager;
    private readonly IBatchDelivery _batchDelivery;

    public BatchAdminController(IBatchDisplayManager batchDisplayManager, IBatchDelivery batchDelivery)
    {
        _batchDisplayManager = batchDisplayManager;
        _batchDelivery = batchDelivery;
    }

    [HttpGet]
    public IActionResult BatchOrderAdmin()
    {
        ViewData["Message"] = TempData["BatchMessage"]?.ToString();
        var batches = _batchDisplayManager.GetBatchesForDisplay();
        return View($"{ViewRoot}BatchOrderAdmin.cshtml", batches);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateOrderAndOption(int orderId, bool useBatchShipping)
    {
        if (useBatchShipping)
        {
            try
            {
                _batchDelivery.consolidateOrderToBatch(orderId.ToString());
                TempData["BatchMessage"] = $"Order {orderId} consolidated into a pending batch.";
            }
            catch (Exception ex)
            {
                TempData["BatchMessage"] = ex.Message;
            }
        }
        else
        {
            TempData["BatchMessage"] = $"Order {orderId} created without batch shipping (demo mode).";
        }

        return RedirectToAction(nameof(BatchOrderAdmin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult MarkBatchesAsShipped(string batchIdsCsv)
    {
        var batchIds = (batchIdsCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        var success = _batchDelivery.markBatchesAsShipped(batchIds);
        TempData["BatchMessage"] = success
            ? "Selected batches were marked as shipped out."
            : "Unable to mark one or more batches as shipped.";

        return RedirectToAction(nameof(BatchOrderAdmin));
    }
}
