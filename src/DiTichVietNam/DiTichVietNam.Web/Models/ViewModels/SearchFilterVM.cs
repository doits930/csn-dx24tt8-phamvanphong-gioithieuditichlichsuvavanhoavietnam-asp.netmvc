using DiTichVietNam.Web.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DiTichVietNam.Web.Models.ViewModels;

public class SearchFilterVM
{
    public const int PageSize = 12;

    private int _page = 1;

    [FromQuery(Name = "tuKhoa")]
    public string? Keyword { get; set; }

    [FromQuery(Name = "tinh")]
    public string? ProvinceSlug { get; set; }

    [FromQuery(Name = "loai")]
    public string? TypeSlug { get; set; }

    [FromQuery(Name = "cap")]
    public RankingLevel? Ranking { get; set; }

    [FromQuery(Name = "trang")]
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    [BindNever]
    public string? PageTitle { get; set; }

    public bool HasAnyFilter =>
        !string.IsNullOrWhiteSpace(Keyword)
        || !string.IsNullOrWhiteSpace(ProvinceSlug)
        || !string.IsNullOrWhiteSpace(TypeSlug)
        || Ranking.HasValue;

    public Dictionary<string, string?> ToRouteValues()
    {
        var values = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(Keyword))
        {
            values["tuKhoa"] = Keyword.Trim();
        }
        if (!string.IsNullOrWhiteSpace(ProvinceSlug))
        {
            values["tinh"] = ProvinceSlug;
        }
        if (!string.IsNullOrWhiteSpace(TypeSlug))
        {
            values["loai"] = TypeSlug;
        }
        if (Ranking.HasValue)
        {
            values["cap"] = ((int)Ranking.Value).ToString();
        }
        return values;
    }
}
