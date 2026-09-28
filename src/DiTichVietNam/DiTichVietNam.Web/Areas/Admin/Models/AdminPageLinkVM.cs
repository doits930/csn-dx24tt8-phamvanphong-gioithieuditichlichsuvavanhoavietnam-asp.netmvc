namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminPageLinkVM
{
    public string Label { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public bool OpensNewTab { get; set; }
}
