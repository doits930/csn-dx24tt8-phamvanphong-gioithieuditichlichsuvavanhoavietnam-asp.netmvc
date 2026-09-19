using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Provinces;
using DiTichVietNam.Web.Services.RelicTypes;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.ViewComponents;

public class MainMenuViewComponent : ViewComponent
{
    private readonly IProvinceService _provinceService;
    private readonly IRelicTypeService _relicTypeService;

    public MainMenuViewComponent(IProvinceService provinceService, IRelicTypeService relicTypeService)
    {
        _provinceService = provinceService;
        _relicTypeService = relicTypeService;
    }

    public async Task<IViewComponentResult> InvokeAsync(bool showSearch)
    {
        var model = new MainMenuVM
        {
            Regions = await _provinceService.GetGroupedByRegionAsync(),
            RelicTypes = await _relicTypeService.GetAllWithCountAsync(),
            Keyword = HttpContext.Request.Query["tuKhoa"],
            ShowSearch = showSearch
        };

        return View(model);
    }
}
