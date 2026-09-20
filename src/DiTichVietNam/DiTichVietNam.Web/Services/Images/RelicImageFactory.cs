using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Services.Relics;

namespace DiTichVietNam.Web.Services.Images;

public static class RelicImageFactory
{
    public const string UploadPathPrefix = "/uploads/relics/";

    public static RelicImageItem ToItem(RelicImage image, string relicName, int position)
    {
        var source = ImageSourceParser.Parse(image.ImageSource);

        return new RelicImageItem
        {
            Id = image.Id,
            Path = image.ImagePath,
            Caption = image.Caption,
            Source = image.ImageSource,
            SourceCredit = source.Credit,
            SourceUrl = source.Url,
            SourceHost = ImageSourceParser.DisplayHost(source.Url),
            IsThumbnail = image.IsThumbnail,
            AltText = BuildAltText(image.Caption, relicName, position)
        };
    }

    private static string BuildAltText(string? caption, string relicName, int position) =>
        string.IsNullOrWhiteSpace(caption) ? $"Ảnh {position} của {relicName}" : caption.Trim();
}
