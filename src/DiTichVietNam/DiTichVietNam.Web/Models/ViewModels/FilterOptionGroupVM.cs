namespace DiTichVietNam.Web.Models.ViewModels;

public class FilterOptionGroupVM
{
    public string DisplayName { get; set; } = string.Empty;
    public List<FilterOptionVM> Options { get; set; } = new();
}
