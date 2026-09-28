using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Relics;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class RelicAdminListVM
{
    public List<RelicAdminRow> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public SearchFilterVM Filter { get; set; } = new();
    public RelicFormOptions Options { get; set; } = new();
    public RelicAdminSummary Summary { get; set; } = new();
    public PaginationVM Pagination { get; set; } = new();
    public string ListUrl { get; set; } = string.Empty;
    public string ResultSummary { get; set; } = string.Empty;
    public bool HasAnyFilter => Filter.HasAnyFilter;
}
