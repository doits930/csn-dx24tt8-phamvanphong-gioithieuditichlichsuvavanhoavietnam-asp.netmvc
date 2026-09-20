using System.Diagnostics;
using DiTichVietNam.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class AdminHomeController : AdminBaseController
{
    public const string ErrorPath = "/admin/loi";

    [HttpGet("/admin")]
    public IActionResult Index() => Redirect(AccountController.AdminHomePath);

    [AllowAnonymous]
    [Route(ErrorPath)]
    public IActionResult Error()
    {
        ViewData["Title"] = "Đã xảy ra lỗi";

        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }

    [Route("/admin/{**unknownPath}")]
    public IActionResult UnknownPage() => AdminNotFound();
}
