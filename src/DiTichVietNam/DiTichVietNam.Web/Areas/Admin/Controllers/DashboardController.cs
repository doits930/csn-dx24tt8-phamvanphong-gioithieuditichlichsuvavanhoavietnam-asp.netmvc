using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Services.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class DashboardController : AdminBaseController
{
    public const string IndexPath = "/admin/thong-ke";

    private readonly IStatisticsService _statisticsService;

    public DashboardController(IStatisticsService statisticsService)
    {
        _statisticsService = statisticsService;
    }

    [HttpGet(IndexPath)]
    public async Task<IActionResult> Index()
    {
        var data = await _statisticsService.GetDashboardAsync();

        ViewData["Title"] = "Thống kê";

        return View(DashboardFactory.ToVM(data));
    }
}
