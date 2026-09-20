namespace DiTichVietNam.Web.Services.Accounts;

public class PasswordChangeInput
{
    public string? CurrentPassword { get; set; }

    public string? NewPassword { get; set; }

    public string? ConfirmPassword { get; set; }
}
