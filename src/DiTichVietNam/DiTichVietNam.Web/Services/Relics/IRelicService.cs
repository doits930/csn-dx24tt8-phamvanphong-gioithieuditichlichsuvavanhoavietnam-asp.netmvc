using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public interface IRelicService
{
    Task<HomeShowcaseVM> GetHomeShowcaseAsync(int heroCount, int featuredCount);

    Task<SearchBarVM> GetSearchBarAsync();

    Task<RelicListVM?> SearchAsync(SearchFilterVM filter);

    Task<RelicDetailVM?> GetDetailAsync(string? slug);

    Task<bool> IncreaseViewCountAsync(int relicId);
}
