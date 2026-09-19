namespace DiTichVietNam.Web.Helpers;

public static class RelicTypeText
{
    private const string CommonPrefix = "Di tích ";

    public static string ShortLabel(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        var trimmed = name.Trim();
        if (!trimmed.StartsWith(CommonPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        var remainder = trimmed[CommonPrefix.Length..].Trim();
        return remainder.Length == 0 ? trimmed : char.ToUpperInvariant(remainder[0]) + remainder[1..];
    }
}
