namespace DiTichVietNam.Web.Models.Entities;

public class RelicImage
{
    public int Id { get; set; }
    public int RelicId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string ImageSource { get; set; } = string.Empty;
    public bool IsThumbnail { get; set; }

    public Relic? Relic { get; set; }
}
