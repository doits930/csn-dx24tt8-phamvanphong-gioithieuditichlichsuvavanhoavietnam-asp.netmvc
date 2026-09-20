using System.Text.RegularExpressions;

namespace DiTichVietNam.Web.Services.Accounts;

public static partial class EmailPattern
{
    public const int MaxEmailLength = 256;

    [GeneratedRegex(@"^[^@\s]+@[^@\s.]+(\.[^@\s.]+)+$")]
    private static partial Regex Shape();

    public static bool IsValid(string value) => Shape().IsMatch(value);
}
