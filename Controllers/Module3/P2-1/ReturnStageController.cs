using Microsoft.AspNetCore.Mvc;
using ProRental.Data.Module3.P2_1.Interfaces;
using ProRental.Domain.Module3.P2_1.Controls;

namespace ProRental.Controllers.Module3.P2_1;

public class ReturnStageController : Controller
{
    private readonly IReturnStageGateway _gateway;
    private readonly ReturnCarbonReportService _reportService;

    public ReturnStageController(
        IReturnStageGateway gateway,
        ReturnStageCalculator calculator,
        ReturnCarbonReportService reportService)
    {
        _gateway = gateway;
        _reportService = reportService;
    }

    // GET: /ReturnStage/StageBreakdown
    // GET: /ReturnStage/StageBreakdown?returnRequestId=1
    public IActionResult StageBreakdown(int? returnRequestId)
    {
        var report = returnRequestId.HasValue
            ? _reportService.GetCarbonReport(returnRequestId.Value)
            : _reportService.GetAllCarbonReport();
        return View("~/Views/Module3/P2-1/StageBreakdown.cshtml", report);
    }

    // POST: /ReturnStage/HandleReturnStage
    [HttpPost]
    public IActionResult HandleReturnStage(int stageId)
    {
        var stage = _gateway.FindById(stageId);
        if (stage == null)
            return NotFound($"Stage {stageId} not found.");

        return RedirectToAction(nameof(StageBreakdown), new { returnRequestId = stage.GetReturnId() });
    }
}
