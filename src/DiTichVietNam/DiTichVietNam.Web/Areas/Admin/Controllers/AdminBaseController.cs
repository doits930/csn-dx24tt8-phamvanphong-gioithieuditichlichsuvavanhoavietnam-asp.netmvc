using DiTichVietNam.Web.Data;
using DiTichVietNam.Web.Services.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SeedData.AdminRoleName)]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public abstract class AdminBaseController : Controller
{
    protected IActionResult AdminNotFound()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        ViewData["Title"] = "Không tìm thấy trang quản trị";

        return View("NotFound");
    }

    protected void AddErrors(List<(string Field, string Message)> errors)
    {
        foreach (var (field, message) in errors)
        {
            ModelState.AddModelError(field, message);
        }
    }

    protected void AddServiceError(ServiceResult result)
    {
        ModelState.AddModelError(result.Field ?? string.Empty, result.Error ?? string.Empty);
    }

    protected string? SafeListReturnUrl(string? returnUrl, string listPath)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl))
        {
            return null;
        }

        var isListUrl = returnUrl == listPath
            || returnUrl.StartsWith(listPath + "?", StringComparison.Ordinal);

        return isListUrl ? returnUrl : null;
    }
}
