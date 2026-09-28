using DiTichVietNam.Web.Models.ViewModels;
using Microsoft.AspNetCore.WebUtilities;

namespace DiTichVietNam.Web.Helpers;

public static class PaginationLinks
{
    private const int NeighbourCount = 1;

    public static string PageUrl(PaginationVM pagination, int page)
    {
        var values = new Dictionary<string, string?>(pagination.RouteValues);
        if (page > 1)
        {
            values[SearchFilterVM.PageKey] = page.ToString();
        }

        return QueryHelpers.AddQueryString(pagination.BasePath, values);
    }

    public static List<int?> PageSequence(PaginationVM pagination)
    {
        var pages = new List<int?>();
        var lastPrinted = 0;

        for (var page = 1; page <= pagination.TotalPages; page++)
        {
            var isEdge = page == 1 || page == pagination.TotalPages;
            var isNearCurrent = Math.Abs(page - pagination.CurrentPage) <= NeighbourCount;
            if (!isEdge && !isNearCurrent)
            {
                continue;
            }

            if (lastPrinted > 0 && page - lastPrinted > 1)
            {
                pages.Add(null);
            }

            pages.Add(page);
            lastPrinted = page;
        }

        return pages;
    }
}
