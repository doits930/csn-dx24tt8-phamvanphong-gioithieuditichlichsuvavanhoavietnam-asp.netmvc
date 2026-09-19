using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public static class RelicSearchQuery
{
    private const int ExcerptRadius = 60;

    public static IQueryable<Relic> ApplyFilter(IQueryable<Relic> source, SearchFilterVM filter, string keywordNoAccent)
    {
        if (!string.IsNullOrWhiteSpace(filter.ProvinceSlug))
        {
            var provinceSlug = filter.ProvinceSlug;
            source = source.Where(r => r.Province!.Slug == provinceSlug);
        }

        if (!string.IsNullOrWhiteSpace(filter.TypeSlug))
        {
            var typeSlug = filter.TypeSlug;
            source = source.Where(r => r.RelicType!.Slug == typeSlug);
        }

        if (filter.Ranking.HasValue)
        {
            var ranking = filter.Ranking.Value;
            source = source.Where(r => r.RankingLevel == ranking);
        }

        if (keywordNoAccent.Length > 0)
        {
            source = source.Where(r =>
                r.NameNoAccent.Contains(keywordNoAccent) || r.DescriptionNoAccent.Contains(keywordNoAccent));
        }

        return source;
    }

    public static string BasePath(SearchFilterVM filter)
    {
        if (filter.ProvinceFromRoute && !string.IsNullOrWhiteSpace(filter.ProvinceSlug))
        {
            return RelicListPaths.ByProvince(filter.ProvinceSlug);
        }

        if (filter.TypeFromRoute && !string.IsNullOrWhiteSpace(filter.TypeSlug))
        {
            return RelicListPaths.ByType(filter.TypeSlug);
        }

        return RelicListPaths.AllRelics;
    }

    public static Dictionary<string, string?> QueryValues(SearchFilterVM filter)
    {
        var values = filter.ToRouteValues();

        if (filter.ProvinceFromRoute)
        {
            values.Remove(SearchFilterVM.ProvinceKey);
        }

        if (filter.TypeFromRoute)
        {
            values.Remove(SearchFilterVM.TypeKey);
        }

        return values;
    }

    public static string? BuildMatchExcerpt(string? description, string keywordNoAccent)
    {
        if (keywordNoAccent.Length == 0 || string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var text = description.Trim();
        var textNoAccent = SlugHelper.RemoveDiacritics(text);
        if (textNoAccent.Length != text.Length)
        {
            return null;
        }

        var index = textNoAccent.IndexOf(keywordNoAccent, StringComparison.Ordinal);
        if (index < 0)
        {
            return null;
        }

        var start = Math.Max(0, index - ExcerptRadius);
        var end = Math.Min(text.Length, index + keywordNoAccent.Length + ExcerptRadius);

        while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
        {
            start--;
        }

        while (end < text.Length && !char.IsWhiteSpace(text[end]))
        {
            end++;
        }

        var excerpt = text[start..end].Trim();
        if (start > 0)
        {
            excerpt = "… " + excerpt;
        }

        if (end < text.Length)
        {
            excerpt += " …";
        }

        return excerpt;
    }
}
