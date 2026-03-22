using Microsoft.AspNetCore.Mvc;

namespace ProRental.Controllers.Module3.P2_5;

public class CarbonFootprintController : Controller
{
    public IActionResult ProductFootprintView()
    {
        return View("~/Views/Module3/P2-5/ProductFootprintView.cshtml");
    }

    public IActionResult StaffFootprintView()
    {
        return View("~/Views/Module3/P2-5/StaffFootprintView.cshtml");
    }

    public IActionResult BuildingFootprintView()
    {
        return View("~/Views/Module3/P2-5/BuildingFootprintView.cshtml");
    }

    public IActionResult PackagingFootprintView()
    {
        return View("~/Views/Module3/P2-5/PackagingFootprintView.cshtml");
    }
}