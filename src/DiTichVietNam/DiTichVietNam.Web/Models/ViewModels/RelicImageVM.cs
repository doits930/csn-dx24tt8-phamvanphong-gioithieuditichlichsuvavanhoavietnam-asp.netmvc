namespace DiTichVietNam.Web.Models.ViewModels;

public class RelicImageVM
{
    public int Position { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public ImageSourceVM Source { get; set; } = new();
    public string AltText { get; set; } = string.Empty;

    public bool HasCaption => !string.IsNullOrWhiteSpace(Caption);
}
