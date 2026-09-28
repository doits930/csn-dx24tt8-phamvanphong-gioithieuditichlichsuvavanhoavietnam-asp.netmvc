namespace DiTichVietNam.Web.Services.Relics;

public class RelicAdminListResult
{
    public List<RelicAdminRow> Rows { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public RelicFormOptions Options { get; set; } = new();
    public RelicAdminSummary Summary { get; set; } = new();
}
