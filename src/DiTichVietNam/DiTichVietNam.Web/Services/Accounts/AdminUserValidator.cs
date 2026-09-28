using Microsoft.AspNetCore.Identity;

namespace DiTichVietNam.Web.Services.Accounts;

public static class AdminUserValidator
{
    public const string EmailField = "Email";
    public const string PasswordField = "Password";
    public const string ConfirmPasswordField = "ConfirmPassword";
    public const string CurrentPasswordField = "CurrentPassword";
    public const string NewPasswordField = "NewPassword";

    public const string EmailRequired = "Nhập địa chỉ thư điện tử.";
    public const string EmailTooLong = "Địa chỉ thư điện tử tối đa 256 ký tự.";
    public const string EmailFormat = "Địa chỉ thư điện tử chưa đúng dạng.";
    public const string PasswordRequired = "Nhập mật khẩu.";
    public const string NewPasswordRequired = "Nhập mật khẩu mới.";
    public const string CurrentPasswordRequired = "Nhập mật khẩu hiện tại.";
    public const string PasswordTooLong = "Mật khẩu tối đa 100 ký tự.";
    public const string ConfirmRequired = "Nhập lại mật khẩu.";
    public const string ConfirmMismatch = "Hai lần nhập mật khẩu chưa khớp nhau.";
    public const string NewPasswordSameAsCurrent = "Mật khẩu mới phải khác mật khẩu hiện tại.";

    public static List<(string Field, string Message)> ValidateCreate(
        AdminUserCreateInput input,
        PasswordPolicy policy,
        AccountNamePolicy namePolicy)
    {
        var errors = new List<(string Field, string Message)>();

        AddEmailErrors(errors, input.Email, namePolicy);
        AddPasswordErrors(errors, PasswordField, input.Password, PasswordRequired, policy);
        AddConfirmErrors(errors, input.Password, input.ConfirmPassword);

        return errors;
    }

    public static List<(string Field, string Message)> ValidateReset(PasswordResetInput input, PasswordPolicy policy)
    {
        var errors = new List<(string Field, string Message)>();

        AddPasswordErrors(errors, PasswordField, input.Password, NewPasswordRequired, policy);
        AddConfirmErrors(errors, input.Password, input.ConfirmPassword);

        return errors;
    }

    public static List<(string Field, string Message)> ValidateChange(PasswordChangeInput input, PasswordPolicy policy)
    {
        var errors = new List<(string Field, string Message)>();

        if (string.IsNullOrEmpty(input.CurrentPassword))
        {
            errors.Add((CurrentPasswordField, CurrentPasswordRequired));
        }
        else if (input.CurrentPassword.Length > PasswordPolicy.MaxPasswordLength)
        {
            errors.Add((CurrentPasswordField, PasswordTooLong));
        }

        AddPasswordErrors(errors, NewPasswordField, input.NewPassword, NewPasswordRequired, policy);

        if (!string.IsNullOrEmpty(input.NewPassword) && input.NewPassword == input.CurrentPassword)
        {
            errors.Add((NewPasswordField, NewPasswordSameAsCurrent));
        }

        AddConfirmErrors(errors, input.NewPassword, input.ConfirmPassword);

        return errors;
    }

    public static (string Field, string Message) Translate(IdentityError error) => error.Code switch
    {
        "DuplicateUserName" or "DuplicateEmail" => (EmailField, "Địa chỉ thư điện tử này đã có tài khoản."),
        "InvalidUserName" => (EmailField, AccountNamePolicy.CharacterMessage),
        "InvalidEmail" => (EmailField, EmailFormat),
        "PasswordTooShort" or "PasswordRequiresDigit" or "PasswordRequiresLower"
            or "PasswordRequiresUpper" or "PasswordRequiresNonAlphanumeric" or "PasswordRequiresUniqueChars"
            => (PasswordField, "Mật khẩu chưa đạt yêu cầu của hệ thống."),
        "PasswordMismatch" => (CurrentPasswordField, "Mật khẩu hiện tại không đúng."),
        _ => (string.Empty, "Không lưu được tài khoản. Thử lại sau ít phút.")
    };

    private static void AddEmailErrors(
        List<(string Field, string Message)> errors,
        string? email,
        AccountNamePolicy namePolicy)
    {
        var value = email?.Trim();
        if (string.IsNullOrEmpty(value))
        {
            errors.Add((EmailField, EmailRequired));
        }
        else if (value.Length > EmailPattern.MaxEmailLength)
        {
            errors.Add((EmailField, EmailTooLong));
        }
        else if (!namePolicy.IsAllowed(value))
        {
            errors.Add((EmailField, AccountNamePolicy.CharacterMessage));
        }
        else if (!EmailPattern.IsValid(value))
        {
            errors.Add((EmailField, EmailFormat));
        }
    }

    private static void AddPasswordErrors(
        List<(string Field, string Message)> errors,
        string field,
        string? password,
        string requiredMessage,
        PasswordPolicy policy)
    {
        if (string.IsNullOrEmpty(password))
        {
            errors.Add((field, requiredMessage));
            return;
        }

        if (password.Length > PasswordPolicy.MaxPasswordLength)
        {
            errors.Add((field, PasswordTooLong));
            return;
        }

        var missing = policy.FindMissing(password);
        if (missing.Count > 0)
        {
            errors.Add((field, PasswordPolicy.MissingMessage(missing)));
        }
    }

    private static void AddConfirmErrors(List<(string Field, string Message)> errors, string? password, string? confirm)
    {
        if (string.IsNullOrEmpty(confirm))
        {
            errors.Add((ConfirmPasswordField, ConfirmRequired));
        }
        else if (!string.IsNullOrEmpty(password) && confirm != password)
        {
            errors.Add((ConfirmPasswordField, ConfirmMismatch));
        }
    }
}
