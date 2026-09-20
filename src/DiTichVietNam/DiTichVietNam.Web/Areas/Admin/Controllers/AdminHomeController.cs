using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Areas.Admin.Controllers;

public class AdminHomeController : AdminBaseController
{
    [HttpGet("/admin")]
    public IActionResult Index() => Redirect(AccountController.AdminHomePath);

    [Route("/admin/{**unknownPath}")]
    public IActionResult UnknownPage() => AdminNotFound();
}
