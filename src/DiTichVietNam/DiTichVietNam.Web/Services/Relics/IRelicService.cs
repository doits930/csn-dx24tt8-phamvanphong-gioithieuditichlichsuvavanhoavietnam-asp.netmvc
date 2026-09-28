using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Common;

namespace DiTichVietNam.Web.Services.Relics;

public interface IRelicService
{
    Task<HomeShowcaseVM> GetHomeShowcaseAsync(int heroCount, int featuredCount);

    Task<SearchBarVM> GetSearchBarAsync();

    Task<RelicListVM?> SearchAsync(SearchFilterVM filter);

    Task<RelicDetailVM?> GetDetailAsync(string? slug);

    Task<bool> IncreaseViewCountAsync(int relicId);

    Task<int> CountAsync();

    Task<RelicAdminListResult> SearchForAdminAsync(SearchFilterVM filter, int pageSize);

    Task<List<RelicSuggestion>> SuggestForAdminAsync(string? keyword, int limit);

    Task<RelicFormOptions> GetFormOptionsAsync();

    Task<RelicAdminSummary> GetAdminSummaryAsync();

    Task<RelicEditData?> GetForEditAsync(int id);

    Task<ServiceResult<int>> CreateAsync(RelicInput input);

    Task<ServiceResult> UpdateAsync(int id, RelicInput input);

    Task<ServiceResult> DeleteAsync(int id);
}
