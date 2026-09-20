using DiTichVietNam.Web.Services.Relics;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class RelicSuggestionFactory
{
    public static List<RelicSuggestionVM> ToViewModels(List<RelicSuggestion> items, string editUrlPrefix)
        => items.Select(item => new RelicSuggestionVM
        {
            Name = item.Name,
            ProvinceName = item.ProvinceName,
            ThumbnailUrl = string.IsNullOrWhiteSpace(item.ThumbnailPath) ? null : item.ThumbnailPath,
            EditUrl = $"{editUrlPrefix}{item.Id}"
        }).ToList();
}
