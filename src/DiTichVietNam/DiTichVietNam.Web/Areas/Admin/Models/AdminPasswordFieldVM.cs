using DiTichVietNam.Web.Services.Accounts;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminPasswordFieldVM
{
    public string FieldId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public string Autocomplete { get; set; } = "new-password";

    public string? Error { get; set; }

    public IReadOnlyList<PasswordRule> Rules { get; set; } = new List<PasswordRule>();

    public bool HasRules => Rules.Count > 0;
}
