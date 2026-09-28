namespace DiTichVietNam.Web.Services.Statistics;

public interface IStatisticsService
{
    Task<DashboardStatistics> GetDashboardAsync();
}
