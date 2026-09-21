using DiTichVietNam.Web.Services.Accounts;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class ChangePasswordVM
{
    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }

    public string? ConfirmPassword { get; set; }

    public string AccountEmail { get; set; } = string.Empty;

    public IReadOnlyList<PasswordRule> PasswordRules { get; set; } = new List<PasswordRule>();
}
