using Microsoft.AspNetCore.Mvc;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module3.P2_1;
using ProRental.Models.Module3.P2_1;

namespace ProRental.Controllers;

public sealed class BatchDeliveryController : Controller
{
    private const string ViewRoot = "~/Views/Module3/P2-1/BatchDelivery/";

    private readonly IBatchDisplayManager _batchDisplayManager;
    private readonly IBatchDelivery _batchDelivery;

    public BatchDeliveryController(IBatchDisplayManager batchDisplayManager, IBatchDelivery batchDelivery)
    {
        _batchDisplayManager = batchDisplayManager;
        _batchDelivery = batchDelivery;
    }

    [HttpGet]
    public async Task<IActionResult> BatchDeliveryView(CancellationToken cancellationToken)
    {
        var model = await BuildPageModel(cancellationToken);
        return View($"{ViewRoot}BatchDeliveryView.cshtml", model);
    }

    [HttpGet]
    public async Task<IActionResult> BatchOperations(CancellationToken cancellationToken)
    {
        var model = await BuildPageModel(cancellationToken);
        return View($"{ViewRoot}BatchOperations.cshtml", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrderForBatch(int orderId, bool useBatchShipping, CancellationToken cancellationToken)
    {
        if (!useBatchShipping)
        {
            TempData["BatchMessage"] = $"Order '{orderId}' was created without batch shipping.";
            return RedirectToAction(nameof(BatchOperations));
        }

        try
        {
            await _batchDelivery.ConsolidateOrderToBatch(orderId.ToString(), cancellationToken);
            TempData["BatchMessage"] = $"Order '{orderId}' was consolidated into a pending batch.";
        }
        catch (Exception ex)
        {
            TempData["BatchError"] = ex.Message;
        }

        return RedirectToAction(nameof(BatchOperations));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShipBatches(string batchIdsCsv, CancellationToken cancellationToken)
    {
        var batchIds = (batchIdsCsv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (batchIds.Length == 0)
        {
            TempData["BatchError"] = "Please provide at least one batch id.";
            return RedirectToAction(nameof(BatchOperations));
        }

        var updated = await _batchDelivery.MarkBatchesAsShipped(batchIds, cancellationToken);
        TempData[updated ? "BatchMessage" : "BatchError"] = updated
            ? "Selected batches were marked as shipped out."
            : "No eligible pending batches were updated.";

        return RedirectToAction(nameof(BatchOperations));
    }

    private async Task<BatchDeliveryPageViewModel> BuildPageModel(CancellationToken cancellationToken)
    {
        var batches = await _batchDisplayManager.GetBatchesForDisplay(cancellationToken);
        var displayItems = batches.Select(MapBatch).OrderBy(item => item.BatchId).ToList();

        return new BatchDeliveryPageViewModel
        {
            Batches = displayItems,
            TotalCarbonSavingsKg = Math.Round(displayItems.Sum(item => item.CarbonSavingsKg), 2, MidpointRounding.AwayFromZero)
        };
    }

    private static BatchDisplayItem MapBatch(DeliveryBatch batch)
    {
        return new BatchDisplayItem(
            batch.GetDeliveryBatchIdentifier(),
            batch.GetSourceHub(),
            batch.GetDestinationAddress(),
            batch.GetDeliveryBatchStatus().ToString(),
            batch.GetTotalOrders(),
            batch.GetBatchWeightKg(),
            batch.GetCarbonSavings(),
            string.Join(", ", batch.GetListOfOrders().OrderBy(orderId => orderId)));
    }
}
