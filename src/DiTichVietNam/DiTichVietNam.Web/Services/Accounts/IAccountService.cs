using System.Security.Claims;
using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.Accounts;

public interface IAccountService
{
    Task<ServiceResult<LoginStatus>> SignInAsync(string? userName, string? password);

    Task SignOutAsync(ClaimsPrincipal principal);

    bool IsAdminSignedIn(ClaimsPrincipal principal);

    string? GetDisplayName(ClaimsPrincipal principal);

    string? GetUserId(ClaimsPrincipal principal);
}
