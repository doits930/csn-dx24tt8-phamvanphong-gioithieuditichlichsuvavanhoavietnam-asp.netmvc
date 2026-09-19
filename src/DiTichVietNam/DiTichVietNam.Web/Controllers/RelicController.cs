using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Relics;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Controllers;

public class RelicController : Controller
{
    private readonly IRelicService _relicService;

    public RelicController(IRelicService relicService)
    {
        _relicService = relicService;
    }

    [HttpGet("/di-tich")]
    public Task<IActionResult> Index(SearchFilterVM filter) => ShowListAsync(filter);

    [HttpGet("/tinh/{provinceSlug}")]
    public Task<IActionResult> ByProvince(string provinceSlug, SearchFilterVM filter)
    {
        filter.ProvinceSlug = provinceSlug;
        filter.ProvinceFromRoute = true;
        return ShowListAsync(filter);
    }

    [HttpGet("/loai/{typeSlug}")]
    public Task<IActionResult> ByType(string typeSlug, SearchFilterVM filter)
    {
        filter.TypeSlug = typeSlug;
        filter.TypeFromRoute = true;
        return ShowListAsync(filter);
    }

    private async Task<IActionResult> ShowListAsync(SearchFilterVM filter)
    {
        var model = await _relicService.SearchAsync(filter);
        if (model is null)
        {
            return NotFound();
        }

        ViewData["Title"] = model.PageTitle;
        ViewData["HideNavSearch"] = true;

        return View("Index", model);
    }
}
