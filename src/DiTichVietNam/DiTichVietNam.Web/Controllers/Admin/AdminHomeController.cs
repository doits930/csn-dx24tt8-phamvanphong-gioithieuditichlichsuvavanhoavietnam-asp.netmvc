using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Controllers.Admin;

public class AdminHomeController : AdminBaseController
{
    [HttpGet("/admin")]
    public IActionResult Index() => Redirect(AccountController.AdminHomePath);
}
