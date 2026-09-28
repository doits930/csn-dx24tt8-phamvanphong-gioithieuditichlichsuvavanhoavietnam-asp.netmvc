namespace DiTichVietNam.Web.Services.Images;

public class RelicImageItem
{
    public int Id { get; set; }
    public string Path { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string Source { get; set; } = string.Empty;
    public string SourceCredit { get; set; } = string.Empty;
    public string? SourceUrl { get; set; }
    public string SourceHost { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; }
    public string AltText { get; set; } = string.Empty;
}
