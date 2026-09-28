using DiTichVietNam.Web.Services.RelicTypes;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicTypeListVM
{
    public List<RelicTypeAdminRow> Rows { get; set; } = new();
    public RelicTypeAdminSummary Summary { get; set; } = new();
    public string ResultSummary { get; set; } = string.Empty;
}
