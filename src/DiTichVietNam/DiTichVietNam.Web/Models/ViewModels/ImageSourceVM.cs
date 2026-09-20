namespace DiTichVietNam.Web.Models.ViewModels;

public class ImageSourceVM
{
    public string Credit { get; set; } = string.Empty;
    public string? Url { get; set; }

    public bool HasCredit => !string.IsNullOrWhiteSpace(Credit);
    public bool HasLink => !string.IsNullOrWhiteSpace(Url);
    public bool HasAnything => HasCredit || HasLink;
}
