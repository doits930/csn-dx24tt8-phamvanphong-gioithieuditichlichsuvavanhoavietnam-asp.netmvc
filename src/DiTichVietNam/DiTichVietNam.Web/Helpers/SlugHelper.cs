using System.Globalization;
using System.Text;

namespace DiTichVietNam.Web.Helpers;

public static class SlugHelper
{
    public static string RemoveDiacritics(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        var withoutStrokedD = input.Replace('đ', 'd').Replace('Đ', 'D');
        var decomposed = withoutStrokedD.Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
    }

    public static string ToSlug(string? input)
    {
        var text = RemoveDiacritics(input);
        if (text.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);
        var lastCharWasDash = false;
        foreach (var character in text)
        {
            var isPlainLetterOrDigit = character < 128 && char.IsLetterOrDigit(character);
            if (isPlainLetterOrDigit)
            {
                builder.Append(character);
                lastCharWasDash = false;
            }
            else if (!lastCharWasDash && builder.Length > 0)
            {
                builder.Append('-');
                lastCharWasDash = true;
            }
        }

        return builder.ToString().Trim('-');
    }
}
