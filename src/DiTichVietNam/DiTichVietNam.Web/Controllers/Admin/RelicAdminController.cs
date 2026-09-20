using DiTichVietNam.Web.Services.Relics;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Controllers.Admin;

public class RelicAdminController : AdminBaseController
{
    private readonly IRelicService _relicService;

    public RelicAdminController(IRelicService relicService)
    {
        _relicService = relicService;
    }

    [HttpGet("/admin/di-tich")]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "Quản lý di tích";

        return View(await _relicService.CountAsync());
    }
}
