using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class AdminHomeController : AdminBaseController
{
    [HttpGet("/admin")]
    public IActionResult Index() => Redirect(AccountController.AdminHomePath);

    [Route("/admin/{**unknownPath}")]
    public IActionResult UnknownPage()
    {
        Response.StatusCode = StatusCodes.Status404NotFound;
        ViewData["Title"] = "Không tìm thấy trang quản trị";
        return View("NotFound");
    }
}
