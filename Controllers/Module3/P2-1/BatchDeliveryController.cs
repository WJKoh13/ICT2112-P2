using Microsoft.AspNetCore.Mvc;
using ProRental.Domain.Entities;
using ProRental.Interfaces.Module3.P2_1;

namespace ProRental.Controllers;

public sealed class BatchDeliveryController : Controller
{
    private const string ViewRoot = "~/Views/Module3/P2-1/";

    private readonly IBatchDisplayManager _batchDisplayManager;

    public BatchDeliveryController(IBatchDisplayManager batchDisplayManager)
    {
        _batchDisplayManager = batchDisplayManager;
    }

    [HttpGet]
    [ActionName("BatchDeliveryView")]
    public IActionResult DisplayBatchTable()
    {
        ViewData["Message"] = TempData["BatchMessage"]?.ToString();
        var batches = _batchDisplayManager.GetBatchesForDisplay();

        var orderIdsByBatch = _batchDisplayManager.GetOrderIdsByBatchForDisplay(batches);

        ViewData["OrderIdsByBatch"] = orderIdsByBatch;

        return View($"{ViewRoot}BatchDeliveryView.cshtml", batches);
    }

    [HttpGet]
    public IActionResult RefreshBatchView()
    {
        return RedirectToAction("BatchDeliveryView");
    }
}
