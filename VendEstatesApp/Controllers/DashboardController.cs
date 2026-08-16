using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendEstatesApp.Constants;
using VendEstatesApp.Services;

namespace VendEstatesApp.Controllers;

[Authorize(Roles = Roles.All)]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _dashboardService.GetSummaryAsync();
        return View(summary);
    }
}
