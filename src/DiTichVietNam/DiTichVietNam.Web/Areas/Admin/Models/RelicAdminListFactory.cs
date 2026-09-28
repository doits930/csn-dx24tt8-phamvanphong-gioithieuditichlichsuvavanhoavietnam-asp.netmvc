using System.Globalization;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Relics;
using Microsoft.AspNetCore.WebUtilities;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class RelicAdminListFactory
{
    public static RelicAdminListVM ToListVM(RelicAdminListResult result, SearchFilterVM filter, string basePath)
    {
        var routeValues = filter.ToRouteValues();

        return new RelicAdminListVM
        {
            Rows = result.Rows,
            TotalCount = result.TotalCount,
            Filter = filter,
            Options = result.Options,
            Summary = result.Summary,
            ResultSummary = BuildSummary(result.TotalCount, filter, result.Options),
            ListUrl = BuildListUrl(basePath, routeValues, result.CurrentPage),
            Pagination = new PaginationVM
            {
                CurrentPage = result.CurrentPage,
                TotalPages = result.TotalPages,
                BasePath = basePath,
                RouteValues = routeValues
            }
        };
    }

    private static string BuildSummary(int totalCount, SearchFilterVM filter, RelicFormOptions options)
    {
        var noun = ResultNoun(filter.TypeSlug, options);

        if (!filter.HasAnyFilter)
        {
            return totalCount == 0
                ? $"Chưa có {noun} nào."
                : $"Đang có {totalCount} {noun}.";
        }

        return totalCount == 0
            ? $"Không có {noun} nào khớp tiêu chí đang chọn."
            : $"{totalCount} {noun} khớp tiêu chí đang chọn.";
    }

    private static string ResultNoun(string? typeSlug, RelicFormOptions options)
    {
        if (string.IsNullOrWhiteSpace(typeSlug))
        {
            return "mục";
        }

        var selected = options.RelicTypes.FirstOrDefault(t => t.Slug == typeSlug);

        return selected is not null && !IntangibleHeritage.IsIntangible(selected.Slug, selected.Name)
            ? "di tích"
            : "mục";
    }

    private static string BuildListUrl(string basePath, Dictionary<string, string?> routeValues, int currentPage)
    {
        var values = new Dictionary<string, string?>(routeValues);
        if (currentPage > 1)
        {
            values[SearchFilterVM.PageKey] = currentPage.ToString(CultureInfo.InvariantCulture);
        }

        return QueryHelpers.AddQueryString(basePath, values);
    }
}
