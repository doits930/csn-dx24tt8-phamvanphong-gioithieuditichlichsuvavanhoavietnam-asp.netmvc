using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Provinces;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class ProvinceController : AdminBaseController
{
    public const string ListPath = "/admin/tinh-thanh";
    public const string CreatePath = "/admin/tinh-thanh/them";
    public const string EditPath = "/admin/tinh-thanh/sua/{id:int}";
    public const string DeletePath = "/admin/tinh-thanh/xoa/{id:int}";

    private const string CreateTitle = "Thêm tỉnh thành";
    private const string EditTitle = "Sửa tỉnh thành";

    private readonly IProvinceService _provinceService;

    public ProvinceController(IProvinceService provinceService)
    {
        _provinceService = provinceService;
    }

    [HttpGet(ListPath)]
    public async Task<IActionResult> Index([FromQuery(Name = SearchFilterVM.KeywordKey)] string? keyword)
    {
        var result = await _provinceService.GetAdminListAsync(keyword);

        ViewData["Title"] = "Quản lý tỉnh thành";

        return View(ProvinceAdminFactory.ToListVM(result, keyword, BuildListUrl(keyword)));
    }

    [HttpGet(CreatePath)]
    public IActionResult Create([FromQuery] string? returnUrl) =>
        ShowForm(ProvinceAdminFactory.NewForm(SafeListReturnUrl(returnUrl, ListPath)), CreateTitle);

    [HttpPost(CreatePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] ProvinceFormVM vm)
    {
        vm.Id = 0;
        vm.Slug = string.Empty;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        var input = ProvinceAdminFactory.ToInput(vm);
        AddErrors(ProvinceFormValidator.Validate(input));

        if (ModelState.IsValid)
        {
            var result = await _provinceService.CreateAsync(input);
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã thêm tỉnh thành {vm.Name?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            AddServiceError(result);
        }

        return ShowForm(vm, CreateTitle);
    }

    [HttpGet(EditPath)]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _provinceService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        return ShowForm(ProvinceAdminFactory.ToFormVM(data, SafeListReturnUrl(returnUrl, ListPath)), EditTitle);
    }

    [HttpPost(EditPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] ProvinceFormVM vm)
    {
        var current = await _provinceService.GetForEditAsync(id);
        if (current is null)
        {
            return AdminNotFound();
        }

        vm.Id = id;
        vm.Slug = current.Slug;
        vm.RelicCount = current.RelicCount;
        vm.HasMapShape = current.HasMapShape;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        var input = ProvinceAdminFactory.ToInput(vm);
        AddErrors(ProvinceFormValidator.Validate(input));

        if (ModelState.IsValid)
        {
            var result = await _provinceService.UpdateAsync(id, input);
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã lưu thay đổi của tỉnh thành {vm.Name?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            AddServiceError(result);
        }

        return ShowForm(vm, EditTitle);
    }

    [HttpGet(DeletePath)]
    public async Task<IActionResult> ConfirmDelete([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _provinceService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        ViewData["Title"] = "Xóa tỉnh thành";

        return View("Delete", ProvinceAdminFactory.ToDeleteVM(data, SafeListReturnUrl(returnUrl, ListPath)));
    }

    [HttpPost(DeletePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _provinceService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        var result = await _provinceService.DeleteAsync(id);
        if (result.Success)
        {
            TempData["AdminSuccess"] = $"Đã xóa tỉnh thành {data.Name}.";
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(SafeListReturnUrl(returnUrl, ListPath) ?? ListPath);
    }

    private static string BuildListUrl(string? keyword) =>
        string.IsNullOrWhiteSpace(keyword)
            ? ListPath
            : $"{ListPath}?{SearchFilterVM.KeywordKey}={Uri.EscapeDataString(keyword.Trim())}";

    private IActionResult ShowForm(ProvinceFormVM vm, string title)
    {
        ViewData["Title"] = title;

        return View("Form", vm);
    }
}
