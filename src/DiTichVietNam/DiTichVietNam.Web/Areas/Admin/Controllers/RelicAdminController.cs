using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Areas.Admin.Validation;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;
using DiTichVietNam.Web.Services.Relics;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class RelicAdminController : AdminBaseController
{
    public const string ListPath = "/admin/di-tich";
    public const string CreatePath = "/admin/di-tich/them";
    public const string SuggestPath = "/admin/di-tich/goi-y";
    public const string EditPath = "/admin/di-tich/sua/{id:int}";
    public const string EditUrlPrefix = "/admin/di-tich/sua/";
    public const string DeletePath = "/admin/di-tich/xoa/{id:int}";

    private const int AdminPageSize = 20;
    private const int SuggestionCount = 6;
    private const string CreateTitle = "Thêm di tích";
    private const string EditTitle = "Sửa di tích";

    private readonly IRelicService _relicService;

    public RelicAdminController(IRelicService relicService)
    {
        _relicService = relicService;
    }

    [HttpGet(ListPath)]
    public async Task<IActionResult> Index([FromQuery] SearchFilterVM filter)
    {
        var result = await _relicService.SearchForAdminAsync(filter, AdminPageSize);

        ViewData["Title"] = "Quản lý di tích";

        return View(RelicAdminListFactory.ToListVM(result, filter, ListPath));
    }

    [HttpGet(SuggestPath)]
    public async Task<IActionResult> Suggest([FromQuery(Name = SearchFilterVM.KeywordKey)] string? tuKhoa)
    {
        var items = await _relicService.SuggestForAdminAsync(tuKhoa, SuggestionCount);

        return Json(RelicSuggestionFactory.ToViewModels(items, EditUrlPrefix));
    }

    [HttpGet(CreatePath)]
    public async Task<IActionResult> Create([FromQuery] string? returnUrl)
    {
        var options = await _relicService.GetFormOptionsAsync();

        return ShowForm(RelicFormFactory.NewForm(options, SafeListReturnUrl(returnUrl, ListPath)), CreateTitle);
    }

    [HttpPost(CreatePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] RelicFormVM vm)
    {
        vm.Id = 0;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        AddErrors(RelicFormValidator.Validate(vm));

        if (ModelState.IsValid)
        {
            var result = await _relicService.CreateAsync(RelicFormFactory.ToInput(vm));
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã thêm hồ sơ {vm.Name?.Trim()}.";
                TempData["AdminAlertLinkUrl"] = $"/admin/di-tich/{result.Data}/anh";
                TempData["AdminAlertLinkText"] = "Thêm ảnh cho hồ sơ này";

                return Redirect(ListPath);
            }

            AddServiceError(result);
        }

        vm.Options = await _relicService.GetFormOptionsAsync();

        return ShowForm(vm, CreateTitle);
    }

    [HttpGet(EditPath)]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _relicService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        var options = await _relicService.GetFormOptionsAsync();

        return ShowForm(RelicFormFactory.ToFormVM(data, options, SafeListReturnUrl(returnUrl, ListPath)), EditTitle);
    }

    [HttpPost(EditPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] RelicFormVM vm)
    {
        var current = await _relicService.GetForEditAsync(id);
        if (current is null)
        {
            return AdminNotFound();
        }

        vm.Id = id;
        vm.Slug = current.Slug;
        vm.ImageCount = current.ImageCount;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        AddErrors(RelicFormValidator.Validate(vm));

        if (ModelState.IsValid)
        {
            var result = await _relicService.UpdateAsync(id, RelicFormFactory.ToInput(vm));
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã lưu thay đổi của hồ sơ {vm.Name?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            AddServiceError(result);
        }

        vm.Options = await _relicService.GetFormOptionsAsync();

        return ShowForm(vm, EditTitle);
    }

    [HttpGet(DeletePath)]
    public async Task<IActionResult> ConfirmDelete([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _relicService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        ViewData["Title"] = "Xóa di tích";

        return View("Delete", new RelicDeleteVM
        {
            Id = data.Id,
            Name = data.Name,
            ImageCount = data.ImageCount,
            ReturnUrl = SafeListReturnUrl(returnUrl, ListPath)
        });
    }

    [HttpPost(DeletePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _relicService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        var result = await _relicService.DeleteAsync(id);
        if (result.Success)
        {
            TempData["AdminSuccess"] = $"Đã xóa hồ sơ {data.Name}.";
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(SafeListReturnUrl(returnUrl, ListPath) ?? ListPath);
    }

    private IActionResult ShowForm(RelicFormVM vm, string title)
    {
        ViewData["Title"] = title;

        return View("Form", vm);
    }
}
