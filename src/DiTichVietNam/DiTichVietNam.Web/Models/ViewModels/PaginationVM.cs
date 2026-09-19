namespace DiTichVietNam.Web.Models.ViewModels;

public class PaginationVM
{
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public Dictionary<string, string?> RouteValues { get; set; } = new();

    public bool HasPrevious => CurrentPage > 1;
    public bool HasNext => CurrentPage < TotalPages;
}
