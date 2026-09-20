using System.Text.RegularExpressions;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Accounts;

public static partial class LoginValidator
{
    public const int MaxUserNameLength = 256;
    public const int MaxPasswordLength = 100;

    public const string UserNameRequired = "Nhập tài khoản.";
    public const string UserNameFormat = "Tài khoản phải là địa chỉ thư điện tử.";
    public const string UserNameTooLong = "Tài khoản tối đa 256 ký tự.";
    public const string PasswordRequired = "Nhập mật khẩu.";
    public const string PasswordTooLong = "Mật khẩu tối đa 100 ký tự.";

    [GeneratedRegex(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$")]
    private static partial Regex EmailPattern();

    public static List<(string Field, string Message)> Validate(LoginVM vm)
    {
        var errors = new List<(string Field, string Message)>();

        var userName = vm.UserName?.Trim();
        if (string.IsNullOrEmpty(userName))
        {
            errors.Add((nameof(vm.UserName), UserNameRequired));
        }
        else if (userName.Length > MaxUserNameLength)
        {
            errors.Add((nameof(vm.UserName), UserNameTooLong));
        }
        else if (!EmailPattern().IsMatch(userName))
        {
            errors.Add((nameof(vm.UserName), UserNameFormat));
        }

        if (string.IsNullOrEmpty(vm.Password))
        {
            errors.Add((nameof(vm.Password), PasswordRequired));
        }
        else if (vm.Password.Length > MaxPasswordLength)
        {
            errors.Add((nameof(vm.Password), PasswordTooLong));
        }

        return errors;
    }
}
