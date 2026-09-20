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
    public const string EditPath = "/admin/di-tich/sua/{id:int}";
    public const string DeletePath = "/admin/di-tich/xoa/{id:int}";

    private const int AdminPageSize = 20;
    private const string CreateTitle = "Thêm di tích";
    private const string EditTitle = "Sửa di tích";

    private readonly IRelicService _relicService;

    public RelicAdminController(IRelicService relicService)
    {
        _relicService = relicService;
    }

    [HttpGet(ListPath)]
    public async Task<IActionResult> Index(SearchFilterVM filter)
    {
        var result = await _relicService.SearchForAdminAsync(filter, AdminPageSize);

        ViewData["Title"] = "Quản lý di tích";

        return View(RelicAdminListFactory.ToListVM(result, filter, ListPath));
    }

    [HttpGet(CreatePath)]
    public async Task<IActionResult> Create(string? returnUrl)
    {
        var options = await _relicService.GetFormOptionsAsync();

        return ShowForm(RelicFormFactory.NewForm(options, ToListReturnUrl(returnUrl)), CreateTitle);
    }

    [HttpPost(CreatePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RelicFormVM vm)
    {
        vm.Id = 0;
        vm.ReturnUrl = ToListReturnUrl(vm.ReturnUrl);

        AddErrors(RelicFormValidator.Validate(vm));

        if (ModelState.IsValid)
        {
            var result = await _relicService.CreateAsync(RelicFormFactory.ToInput(vm));
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã thêm di tích {vm.Name?.Trim()}.";

                return Redirect(ListPath);
            }

            AddServiceError(result);
        }

        vm.Options = await _relicService.GetFormOptionsAsync();

        return ShowForm(vm, CreateTitle);
    }

    [HttpGet(EditPath)]
    public async Task<IActionResult> Edit(int id, string? returnUrl)
    {
        var data = await _relicService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        var options = await _relicService.GetFormOptionsAsync();

        return ShowForm(RelicFormFactory.ToFormVM(data, options, ToListReturnUrl(returnUrl)), EditTitle);
    }

    [HttpPost(EditPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, RelicFormVM vm)
    {
        var current = await _relicService.GetForEditAsync(id);
        if (current is null)
        {
            return AdminNotFound();
        }

        vm.Id = id;
        vm.Slug = current.Slug;
        vm.ImageCount = current.ImageCount;
        vm.ReturnUrl = ToListReturnUrl(vm.ReturnUrl);

        AddErrors(RelicFormValidator.Validate(vm));

        if (ModelState.IsValid)
        {
            var result = await _relicService.UpdateAsync(id, RelicFormFactory.ToInput(vm));
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã lưu thay đổi của di tích {vm.Name?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            AddServiceError(result);
        }

        vm.Options = await _relicService.GetFormOptionsAsync();

        return ShowForm(vm, EditTitle);
    }

    [HttpGet(DeletePath)]
    public async Task<IActionResult> ConfirmDelete(int id, string? returnUrl)
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
            ReturnUrl = ToListReturnUrl(returnUrl)
        });
    }

    [HttpPost(DeletePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, string? returnUrl)
    {
        var data = await _relicService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        var result = await _relicService.DeleteAsync(id);
        if (result.Success)
        {
            TempData["AdminSuccess"] = $"Đã xóa di tích {data.Name}.";
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(ToListReturnUrl(returnUrl) ?? ListPath);
    }

    private IActionResult ShowForm(RelicFormVM vm, string title)
    {
        ViewData["Title"] = title;

        return View("Form", vm);
    }

    private void AddErrors(List<(string Field, string Message)> errors)
    {
        foreach (var (field, message) in errors)
        {
            ModelState.AddModelError(field, message);
        }
    }

    private void AddServiceError(ServiceResult result)
    {
        ModelState.AddModelError(result.Field ?? string.Empty, result.Error ?? string.Empty);
    }

    private string? ToListReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            return null;
        }

        var isListUrl = returnUrl == ListPath
            || returnUrl.StartsWith(ListPath + "?", StringComparison.Ordinal);

        return isListUrl ? returnUrl : null;
    }
}
