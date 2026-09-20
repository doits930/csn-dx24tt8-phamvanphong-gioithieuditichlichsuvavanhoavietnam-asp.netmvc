namespace DiTichVietNam.Web.Services.Images;

public class RelicImageBoard
{
    public int RelicId { get; set; }
    public string RelicName { get; set; } = string.Empty;
    public string RelicSlug { get; set; } = string.Empty;
    public List<RelicImageItem> Images { get; set; } = new();

    public int RemainingSlots => Math.Max(0, ImageUploadLimits.MaxImagesPerRelic - Images.Count);
    public bool IsFull => RemainingSlots == 0;
}
