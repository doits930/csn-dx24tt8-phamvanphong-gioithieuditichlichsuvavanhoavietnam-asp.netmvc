using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Services.RelicTypes;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class RelicTypeController : AdminBaseController
{
    public const string ListPath = "/admin/loai-di-tich";
    public const string CreatePath = "/admin/loai-di-tich/them";
    public const string EditPath = "/admin/loai-di-tich/sua/{id:int}";
    public const string DeletePath = "/admin/loai-di-tich/xoa/{id:int}";

    private const string CreateTitle = "Thêm loại di tích";
    private const string EditTitle = "Sửa loại di tích";

    private readonly IRelicTypeService _relicTypeService;

    public RelicTypeController(IRelicTypeService relicTypeService)
    {
        _relicTypeService = relicTypeService;
    }

    [HttpGet(ListPath)]
    public async Task<IActionResult> Index()
    {
        var result = await _relicTypeService.GetAdminListAsync();

        ViewData["Title"] = "Quản lý loại di tích";

        return View(RelicTypeAdminFactory.ToListVM(result));
    }

    [HttpGet(CreatePath)]
    public IActionResult Create([FromQuery] string? returnUrl) =>
        ShowForm(RelicTypeAdminFactory.NewForm(SafeListReturnUrl(returnUrl, ListPath)), CreateTitle);

    [HttpPost(CreatePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] RelicTypeFormVM vm)
    {
        vm.Id = 0;
        vm.Slug = string.Empty;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        var input = RelicTypeAdminFactory.ToInput(vm);
        AddErrors(RelicTypeFormValidator.Validate(input));

        if (ModelState.IsValid)
        {
            var result = await _relicTypeService.CreateAsync(input);
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã thêm loại di tích {vm.Name?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            AddServiceError(result);
        }

        return ShowForm(vm, CreateTitle);
    }

    [HttpGet(EditPath)]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _relicTypeService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        return ShowForm(RelicTypeAdminFactory.ToFormVM(data, SafeListReturnUrl(returnUrl, ListPath)), EditTitle);
    }

    [HttpPost(EditPath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, [FromForm] RelicTypeFormVM vm)
    {
        var current = await _relicTypeService.GetForEditAsync(id);
        if (current is null)
        {
            return AdminNotFound();
        }

        vm.Id = id;
        vm.Slug = current.Slug;
        vm.RelicCount = current.RelicCount;
        vm.ReturnUrl = SafeListReturnUrl(vm.ReturnUrl, ListPath);

        var input = RelicTypeAdminFactory.ToInput(vm);
        AddErrors(RelicTypeFormValidator.Validate(input));

        if (ModelState.IsValid)
        {
            var result = await _relicTypeService.UpdateAsync(id, input);
            if (result.Success)
            {
                TempData["AdminSuccess"] = $"Đã lưu thay đổi của loại di tích {vm.Name?.Trim()}.";

                return Redirect(vm.ReturnUrl ?? ListPath);
            }

            AddServiceError(result);
        }

        return ShowForm(vm, EditTitle);
    }

    [HttpGet(DeletePath)]
    public async Task<IActionResult> ConfirmDelete([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _relicTypeService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        ViewData["Title"] = "Xóa loại di tích";

        return View("Delete", RelicTypeAdminFactory.ToDeleteVM(data, SafeListReturnUrl(returnUrl, ListPath)));
    }

    [HttpPost(DeletePath)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromRoute] int id, [FromQuery] string? returnUrl)
    {
        var data = await _relicTypeService.GetForEditAsync(id);
        if (data is null)
        {
            return AdminNotFound();
        }

        var result = await _relicTypeService.DeleteAsync(id);
        if (result.Success)
        {
            TempData["AdminSuccess"] = $"Đã xóa loại di tích {data.Name}.";
        }
        else
        {
            TempData["AdminError"] = result.Error;
        }

        return Redirect(SafeListReturnUrl(returnUrl, ListPath) ?? ListPath);
    }

    private IActionResult ShowForm(RelicTypeFormVM vm, string title)
    {
        ViewData["Title"] = title;

        return View("Form", vm);
    }
}
