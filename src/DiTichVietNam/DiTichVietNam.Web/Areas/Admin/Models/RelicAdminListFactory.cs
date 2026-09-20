using System.Globalization;
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
            ResultSummary = BuildSummary(result.TotalCount, filter.HasAnyFilter),
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

    private static string BuildSummary(int totalCount, bool hasAnyFilter)
    {
        if (!hasAnyFilter)
        {
            return totalCount == 0
                ? "Chưa có di tích nào."
                : $"Đang có {totalCount} di tích.";
        }

        return totalCount == 0
            ? "Không có di tích nào khớp tiêu chí đang chọn."
            : $"{totalCount} di tích khớp tiêu chí đang chọn.";
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
