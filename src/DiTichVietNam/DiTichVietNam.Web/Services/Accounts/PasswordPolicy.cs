using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.Services.Accounts;

public class PasswordPolicy
{
    public const int MaxPasswordLength = 100;

    public const string LengthCode = "length";
    public const string LowercaseCode = "lowercase";
    public const string UppercaseCode = "uppercase";
    public const string DigitCode = "digit";
    public const string SymbolCode = "symbol";
    public const string UniqueCode = "unique";

    private readonly List<PasswordRule> _rules;

    public PasswordPolicy(IOptions<IdentityOptions> options)
    {
        _rules = BuildRules(options.Value.Password);
    }

    public IReadOnlyList<PasswordRule> Rules => _rules;

    public List<string> FindMissing(string? password)
    {
        var value = password ?? string.Empty;

        return _rules
            .Where(rule => !IsMet(rule, value))
            .Select(rule => rule.ShortLabel)
            .ToList();
    }

    public static string MissingMessage(IReadOnlyList<string> missing)
        => $"Mật khẩu còn thiếu: {string.Join(", ", missing)}.";

    private static bool IsMet(PasswordRule rule, string password) => rule.Code switch
    {
        LengthCode => password.Length >= rule.Threshold,
        LowercaseCode => password.Any(char.IsLower),
        UppercaseCode => password.Any(char.IsUpper),
        DigitCode => password.Any(char.IsDigit),
        SymbolCode => password.Any(c => !char.IsLetterOrDigit(c)),
        UniqueCode => password.Distinct().Count() >= rule.Threshold,
        _ => true
    };

    private static List<PasswordRule> BuildRules(PasswordOptions options)
    {
        var rules = new List<PasswordRule>
        {
            new()
            {
                Code = LengthCode,
                Threshold = options.RequiredLength,
                Description = $"Ít nhất {options.RequiredLength} ký tự",
                ShortLabel = $"ít nhất {options.RequiredLength} ký tự"
            }
        };

        if (options.RequireLowercase)
        {
            rules.Add(new PasswordRule
            {
                Code = LowercaseCode,
                Description = "Có chữ thường",
                ShortLabel = "chữ thường"
            });
        }

        if (options.RequireUppercase)
        {
            rules.Add(new PasswordRule
            {
                Code = UppercaseCode,
                Description = "Có chữ in hoa",
                ShortLabel = "chữ in hoa"
            });
        }

        if (options.RequireDigit)
        {
            rules.Add(new PasswordRule
            {
                Code = DigitCode,
                Description = "Có chữ số",
                ShortLabel = "chữ số"
            });
        }

        if (options.RequireNonAlphanumeric)
        {
            rules.Add(new PasswordRule
            {
                Code = SymbolCode,
                Description = "Có ký tự đặc biệt, ví dụ @ hoặc #",
                ShortLabel = "ký tự đặc biệt"
            });
        }

        if (options.RequiredUniqueChars > 1)
        {
            rules.Add(new PasswordRule
            {
                Code = UniqueCode,
                Threshold = options.RequiredUniqueChars,
                Description = $"Có ít nhất {options.RequiredUniqueChars} ký tự khác nhau",
                ShortLabel = $"ít nhất {options.RequiredUniqueChars} ký tự khác nhau"
            });
        }

        return rules;
    }
}
