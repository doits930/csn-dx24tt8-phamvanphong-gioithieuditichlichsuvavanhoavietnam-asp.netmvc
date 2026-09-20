using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public static class ImageSourceParser
{
    private static readonly char[] TrailingMarks = { '.', ',', ';', ':', ')', ']', '"', '\'', '…' };

    public static ImageSourceVM Parse(string? rawSource)
    {
        if (string.IsNullOrWhiteSpace(rawSource))
        {
            return new ImageSourceVM();
        }

        var text = rawSource.Trim();
        var start = FindLinkStart(text);
        if (start < 0)
        {
            return new ImageSourceVM { Credit = Tidy(text) };
        }

        var end = start;
        while (end < text.Length && !char.IsWhiteSpace(text[end]))
        {
            end++;
        }

        var candidate = text[start..end].TrimEnd(TrailingMarks);
        if (!IsWebAddress(candidate))
        {
            return new ImageSourceVM { Credit = Tidy(text) };
        }

        var credit = Tidy(text.Remove(start, end - start));
        return new ImageSourceVM { Credit = credit, Url = candidate };
    }

    public static string? SafeUrl(string? rawUrl)
    {
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return null;
        }

        var candidate = rawUrl.Trim();
        return IsWebAddress(candidate) ? candidate : null;
    }

    public static string DisplayHost(string? rawUrl)
    {
        var safe = SafeUrl(rawUrl);
        if (safe is null || !Uri.TryCreate(safe, UriKind.Absolute, out var uri))
        {
            return string.Empty;
        }

        var host = uri.Host;
        return host.StartsWith("www.", StringComparison.OrdinalIgnoreCase) ? host[4..] : host;
    }

    private static int FindLinkStart(string text)
    {
        var index = text.IndexOf("http", StringComparison.OrdinalIgnoreCase);
        return index;
    }

    private static bool IsWebAddress(string candidate) =>
        Uri.TryCreate(candidate, UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static string Tidy(string text)
    {
        var cleaned = text.Trim().TrimEnd(TrailingMarks).TrimEnd();
        while (cleaned.EndsWith(',') || cleaned.EndsWith(';'))
        {
            cleaned = cleaned[..^1].TrimEnd();
        }

        return cleaned;
    }
}
