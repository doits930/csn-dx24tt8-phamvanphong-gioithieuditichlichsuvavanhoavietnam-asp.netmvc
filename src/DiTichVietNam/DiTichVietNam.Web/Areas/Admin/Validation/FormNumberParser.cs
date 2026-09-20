using System.Globalization;

namespace DiTichVietNam.Web.Areas.Admin.Validation;

public static class FormNumberParser
{
    public static bool TryParseWholeNumber(string? text, out int value)
    {
        value = 0;
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed) || !HasOnlyAsciiDigits(trimmed))
        {
            return false;
        }

        return int.TryParse(trimmed, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDecimalNumber(string? text, out double value)
    {
        value = 0;
        var trimmed = text?.Trim().Replace(',', '.');
        if (string.IsNullOrEmpty(trimmed))
        {
            return false;
        }

        var digits = trimmed[0] is '-' or '+' ? trimmed[1..] : trimmed;
        if (digits.Length == 0 || !HasOnlyAsciiDigitsOrPoint(digits))
        {
            return false;
        }

        return double.TryParse(
            trimmed,
            NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static bool HasOnlyAsciiDigits(string text) => text.All(char.IsAsciiDigit);

    private static bool HasOnlyAsciiDigitsOrPoint(string text) =>
        text.All(character => char.IsAsciiDigit(character) || character == '.');
}
