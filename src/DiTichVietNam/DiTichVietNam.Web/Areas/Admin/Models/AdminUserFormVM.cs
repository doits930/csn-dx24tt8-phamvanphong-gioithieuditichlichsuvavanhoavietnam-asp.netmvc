using DiTichVietNam.Web.Services.Accounts;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminUserFormVM
{
    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? ConfirmPassword { get; set; }

    public string? ReturnUrl { get; set; }

    public IReadOnlyList<PasswordRule> PasswordRules { get; set; } = new List<PasswordRule>();

    public string AllowedEmailCharacters { get; set; } = string.Empty;
}
