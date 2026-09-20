using DiTichVietNam.Web.Controllers;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Accounts;
using DiTichVietNam.Web.Services.Provinces;
using DiTichVietNam.Web.Services.RelicTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.ViewComponents;

public class MainMenuViewComponent : ViewComponent
{
    private const string LogoutPath = "/tai-khoan/dang-xuat";

    private readonly IProvinceService _provinceService;
    private readonly IRelicTypeService _relicTypeService;
    private readonly IAccountService _accountService;
    private readonly AdminSiteOptions _adminSite;

    public MainMenuViewComponent(
        IProvinceService provinceService,
        IRelicTypeService relicTypeService,
        IAccountService accountService,
        IOptions<AdminSiteOptions> adminSite)
    {
        _provinceService = provinceService;
        _relicTypeService = relicTypeService;
        _accountService = accountService;
        _adminSite = adminSite.Value;
    }

    public async Task<IViewComponentResult> InvokeAsync(bool showSearch)
    {
        var model = new MainMenuVM
        {
            Regions = await _provinceService.GetGroupedByRegionAsync(),
            RelicTypes = await _relicTypeService.GetAllWithCountAsync(),
            Keyword = HttpContext.Request.Query["tuKhoa"],
            ShowSearch = showSearch,
            IsAdminSignedIn = _accountService.IsAdminSignedIn(HttpContext.User)
        };

        if (model.IsAdminSignedIn)
        {
            model.AccountName = _accountService.GetDisplayName(HttpContext.User);
            model.AdminHomeUrl = SiteUrls.OnPort(HttpContext.Request, _adminSite.Port, AccountController.AdminHomePath);
            model.LogoutUrl = SiteUrls.OnPort(HttpContext.Request, _adminSite.Port, LogoutPath);
        }

        return View(model);
    }
}
