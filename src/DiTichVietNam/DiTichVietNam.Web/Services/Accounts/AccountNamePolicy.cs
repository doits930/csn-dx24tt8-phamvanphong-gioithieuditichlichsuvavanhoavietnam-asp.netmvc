using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.Services.Accounts;

public class AccountNamePolicy
{
    public const string CharacterMessage =
        "Địa chỉ email chỉ gồm chữ cái không dấu, chữ số và các ký tự . _ - + @";

    private readonly string _allowedCharacters;

    public AccountNamePolicy(IOptions<IdentityOptions> options)
    {
        _allowedCharacters = options.Value.User.AllowedUserNameCharacters ?? string.Empty;
    }

    public string AllowedCharacters => _allowedCharacters;

    public bool IsAllowed(string value)
        => _allowedCharacters.Length == 0 || value.All(c => _allowedCharacters.Contains(c));
}
