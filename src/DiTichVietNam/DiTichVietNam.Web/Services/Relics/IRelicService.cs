using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public interface IRelicService
{
    Task<HomeShowcaseVM> GetHomeShowcaseAsync(int heroCount, int featuredCount);

    Task<HomeStatsVM> GetHomeStatsAsync();
}
