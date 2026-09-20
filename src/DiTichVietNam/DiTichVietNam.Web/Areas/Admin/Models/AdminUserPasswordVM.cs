using DiTichVietNam.Web.Services.Accounts;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminUserPasswordVM
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Password { get; set; }

    public string? ConfirmPassword { get; set; }

    public string? ReturnUrl { get; set; }

    public bool IsLocked { get; set; }

    public IReadOnlyList<PasswordRule> PasswordRules { get; set; } = new List<PasswordRule>();
}
