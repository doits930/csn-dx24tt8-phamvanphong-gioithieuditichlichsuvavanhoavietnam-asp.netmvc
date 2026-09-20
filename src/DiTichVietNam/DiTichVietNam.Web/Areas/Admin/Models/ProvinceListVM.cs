using DiTichVietNam.Web.Services.Provinces;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class ProvinceListVM
{
    public List<ProvinceAdminRow> Rows { get; set; } = new();
    public ProvinceAdminSummary Summary { get; set; } = new();
    public string? Keyword { get; set; }
    public string ListUrl { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;

    public bool HasKeyword => !string.IsNullOrWhiteSpace(Keyword);
}
