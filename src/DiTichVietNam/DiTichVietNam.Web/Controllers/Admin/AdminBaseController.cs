using DiTichVietNam.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiTichVietNam.Web.Controllers.Admin;

[Authorize(Roles = SeedData.AdminRoleName)]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public abstract class AdminBaseController : Controller
{
}
