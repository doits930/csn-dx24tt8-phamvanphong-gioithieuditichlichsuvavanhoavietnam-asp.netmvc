using DiTichVietNam.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DiTichVietNam.Web.Models.ViewModels;

public class SearchFilterVM
{
    public const int PageSize = 12;
    public const int MaxKeywordLength = 200;
    public const int MaxSuggestionKeywordLength = 100;
    public const string KeywordKey = "tuKhoa";
    public const string ProvinceKey = "tinh";
    public const string TypeKey = "loai";
    public const string RankingKey = "cap";
    public const string PageKey = "trang";

    private const int MaxPageDigits = 9;

    private int? _resolvedPage;

    [FromQuery(Name = KeywordKey)]
    public string? Keyword { get; set; }

    [FromQuery(Name = ProvinceKey)]
    public string? ProvinceSlug { get; set; }

    [FromQuery(Name = TypeKey)]
    public string? TypeSlug { get; set; }

    [FromQuery(Name = RankingKey)]
    public RankingLevel? Ranking { get; set; }

    [FromQuery(Name = PageKey)]
    public string? PageText { get; set; }

    [BindNever]
    public int Page
    {
        get => _resolvedPage ?? ParsePage(PageText);
        set => _resolvedPage = value < 1 ? 1 : value;
    }

    [BindNever]
    public bool ProvinceFromRoute { get; set; }

    [BindNever]
    public bool TypeFromRoute { get; set; }

    public string? NormalizedKeyword
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Keyword))
            {
                return null;
            }

            var trimmed = Keyword.Trim();

            return trimmed.Length > MaxKeywordLength ? trimmed[..MaxKeywordLength] : trimmed;
        }
    }

    public bool HasAnyFilter =>
        !string.IsNullOrWhiteSpace(Keyword)
        || !string.IsNullOrWhiteSpace(ProvinceSlug)
        || !string.IsNullOrWhiteSpace(TypeSlug)
        || Ranking.HasValue;

    public Dictionary<string, string?> ToRouteValues()
    {
        var values = new Dictionary<string, string?>();
        if (NormalizedKeyword is { Length: > 0 } keyword)
        {
            values[KeywordKey] = keyword;
        }
        if (!string.IsNullOrWhiteSpace(ProvinceSlug))
        {
            values[ProvinceKey] = ProvinceSlug;
        }
        if (!string.IsNullOrWhiteSpace(TypeSlug))
        {
            values[TypeKey] = TypeSlug;
        }
        if (Ranking.HasValue)
        {
            values[RankingKey] = ((int)Ranking.Value).ToString();
        }
        return values;
    }

    private static int ParsePage(string? text)
    {
        var digits = text?.Trim();
        if (string.IsNullOrEmpty(digits) || !digits.All(char.IsAsciiDigit))
        {
            return 1;
        }

        if (digits.TrimStart('0').Length > MaxPageDigits)
        {
            return int.MaxValue;
        }

        var page = int.Parse(digits);
        return page < 1 ? 1 : page;
    }
}
