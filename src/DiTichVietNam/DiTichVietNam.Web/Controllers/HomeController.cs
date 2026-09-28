using System.Diagnostics;
using DiTichVietNam.Web.Models;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Provinces;
using DiTichVietNam.Web.Services.RelicTypes;
using DiTichVietNam.Web.Services.Relics;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Controllers;

public class HomeController : Controller
{
    private const int HeroSlideCount = 5;
    private const int FeaturedRelicCount = 5;

    private readonly IRelicService _relicService;
    private readonly IProvinceService _provinceService;
    private readonly IRelicTypeService _relicTypeService;

    public HomeController(
        IRelicService relicService,
        IProvinceService provinceService,
        IRelicTypeService relicTypeService)
    {
        _relicService = relicService;
        _provinceService = provinceService;
        _relicTypeService = relicTypeService;
    }

    public async Task<IActionResult> Index()
    {
        var model = new HomeVM
        {
            Showcase = await _relicService.GetHomeShowcaseAsync(HeroSlideCount, FeaturedRelicCount),
            SearchBar = await _relicService.GetSearchBarAsync(),
            Regions = await _provinceService.GetGroupedByRegionAsync(),
            Map = await _provinceService.GetVietnamMapAsync(),
            RelicTypes = await _relicTypeService.GetAllWithCountAsync()
        };

        ViewData["HideNavSearch"] = true;

        return View(model);
    }

    [Route("/loi/{code:int}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Status(int code)
    {
        if (code < StatusCodes.Status400BadRequest || code > 599)
        {
            return NotFound();
        }

        Response.StatusCode = code;

        if (code != StatusCodes.Status404NotFound)
        {
            return View("Error", new ErrorViewModel());
        }

        ViewData["Title"] = "Không tìm thấy nội dung";

        return View("NotFound", await _relicService.GetSearchBarAsync());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
