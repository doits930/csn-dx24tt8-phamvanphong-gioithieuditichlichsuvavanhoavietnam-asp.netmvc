namespace DiTichVietNam.Web.Services.Accounts;

public class PasswordRule
{
    public string Code { get; init; } = string.Empty;

    public int Threshold { get; init; }

    public string Description { get; init; } = string.Empty;

    public string ShortLabel { get; init; } = string.Empty;
}
