using System.Security.Claims;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.Accounts;

public interface IAccountService
{
    Task<ServiceResult<LoginStatus>> SignInAsync(LoginVM vm);

    Task SignOutAsync();

    bool IsAdminSignedIn(ClaimsPrincipal principal);

    string? GetDisplayName(ClaimsPrincipal principal);
}
