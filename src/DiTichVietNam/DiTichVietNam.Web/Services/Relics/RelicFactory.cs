using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;
using DiTichVietNam.Web.Services.Provinces;

namespace DiTichVietNam.Web.Services.Relics;

public static class RelicFactory
{
    public static RelicCardVM ToCardVM(Relic relic) => ToCardVM(relic, null);

    public static RelicCardVM ToCardVM(Relic relic, string? matchExcerpt) => new()
    {
        MatchExcerpt = matchExcerpt,
        Id = relic.Id,
        Name = relic.Name,
        Slug = relic.Slug,
        ProvinceName = relic.Province?.Name ?? string.Empty,
        TypeName = relic.RelicType?.Name ?? string.Empty,
        RankingLevel = relic.RankingLevel,
        RankingLabel = RankingLevelText.Label(relic.RankingLevel),
        ThumbnailPath = relic.ThumbnailPath
    };

    public static RelicDetailVM ToDetailVM(Relic relic, IReadOnlyList<RelicImage> images)
    {
        var ordered = images
            .OrderByDescending(image => image.IsThumbnail)
            .ThenBy(image => image.Id)
            .ToList();

        var model = new RelicDetailVM
        {
            Id = relic.Id,
            Name = relic.Name,
            Slug = relic.Slug,
            Address = relic.Address,
            ProvinceName = relic.Province?.Name ?? string.Empty,
            ProvinceSlug = relic.Province?.Slug ?? string.Empty,
            RegionName = relic.Province is null ? string.Empty : RegionText.DisplayName(relic.Province.Region),
            TypeName = relic.RelicType?.Name ?? string.Empty,
            TypeSlug = relic.RelicType?.Slug ?? string.Empty,
            RankingLevel = relic.RankingLevel,
            RankingLabel = RankingLevelText.Label(relic.RankingLevel),
            RankingShortLabel = RankingLevelText.ShortLabel(relic.RankingLevel),
            RecognizedYear = relic.RecognizedYear,
            ViewCount = relic.ViewCount,
            Description = relic.Description,
            History = relic.History,
            SourceUrl = ImageSourceParser.SafeUrl(relic.SourceUrl),
            SourceHost = ImageSourceParser.DisplayHost(relic.SourceUrl),
            Timeline = RelicTimelineParser.Parse(relic.History),
            Visit = VisitInfoParser.Parse(relic.VisitInfo)
        };

        var position = 0;
        foreach (var image in ordered)
        {
            model.Images.Add(new RelicImageVM
            {
                Position = position,
                Path = image.ImagePath,
                Caption = image.Caption,
                Source = ImageSourceParser.Parse(image.ImageSource),
                AltText = string.IsNullOrWhiteSpace(image.Caption) ? relic.Name : image.Caption!
            });
            position++;
        }

        model.LocationMap = RelicLocationMapFactory.Build(
            model.ProvinceSlug,
            model.ProvinceName,
            model.RegionName,
            relic.Latitude,
            relic.Longitude);

        ApplyIntroAndBody(model, relic.Description);

        model.Breadcrumbs = BuildDetailBreadcrumbs(model);
        model.SectionLinks = BuildSectionLinks(model);
        model.VisitSummary = BuildVisitSummary(model.Visit);

        return model;
    }

    private static string BuildVisitSummary(VisitCardVM? visit)
    {
        if (visit is null || !visit.HasAnything)
        {
            return string.Empty;
        }

        if (visit.ShowsTicket)
        {
            return visit.HasHeadlineAmount ? $"Vé từ {visit.HeadlineAmount}" : "Có bán vé tham quan";
        }

        return visit.IsFreeEntry ? "Vào cửa miễn phí" : "Thông tin tham quan";
    }

    private static void ApplyIntroAndBody(RelicDetailVM model, string? description)
    {
        var text = SentenceSplitter.Normalize(description);
        model.DescriptionBody = text;
        model.Intro = string.Empty;
        model.LeadSentence = string.Empty;

        if (text.Length == 0)
        {
            return;
        }

        var end = SentenceSplitter.FirstSentenceEnd(text);
        var opening = (end <= 0 ? text : text[..end]).Trim();

        model.LeadSentence = opening.Length <= RelicIntroText.CardMaxLength
            ? opening
            : RelicIntroText.Shorten(opening, RelicIntroText.CardMaxLength);

        if (end <= 0 || end >= text.Length)
        {
            return;
        }

        if (opening.Length > RelicIntroText.CoverMaxLength)
        {
            model.Intro = RelicIntroText.Shorten(opening, RelicIntroText.CoverMaxLength);
            return;
        }

        model.Intro = opening;
        model.DescriptionBody = text[end..].TrimStart();
    }

    private static List<BreadcrumbItemVM> BuildDetailBreadcrumbs(RelicDetailVM model)
    {
        var trail = new List<BreadcrumbItemVM>
        {
            new() { Label = "Trang chủ", Url = "/" },
            new() { Label = "Di tích", Url = RelicListPaths.AllRelics }
        };

        if (!string.IsNullOrWhiteSpace(model.ProvinceSlug))
        {
            trail.Add(new BreadcrumbItemVM
            {
                Label = model.ProvinceName,
                Url = RelicListPaths.ByProvince(model.ProvinceSlug)
            });
        }

        trail.Add(new BreadcrumbItemVM { Label = model.Name });
        return trail;
    }

    private static List<SectionLinkVM> BuildSectionLinks(RelicDetailVM model)
    {
        var links = new List<SectionLinkVM>();

        if (model.HasDescription)
        {
            links.Add(new SectionLinkVM { Label = "Giới thiệu", Anchor = "gioi-thieu" });
        }

        if (model.HasHistory)
        {
            links.Add(new SectionLinkVM { Label = "Lịch sử", Anchor = "lich-su" });
        }

        if (model.HasVisitCard)
        {
            links.Add(new SectionLinkVM { Label = "Tham quan", Anchor = "tham-quan" });
        }

        if (model.HasImages)
        {
            links.Add(new SectionLinkVM { Label = "Hình ảnh", Anchor = "hinh-anh" });
        }

        if (model.HasSource)
        {
            links.Add(new SectionLinkVM { Label = "Nguồn", Anchor = "nguon-tham-khao" });
        }

        return links;
    }

    public static HeroSlideVM ToHeroSlideVM(Relic relic, IReadOnlyList<RelicImage> images)
    {
        var main = images.FirstOrDefault(i => i.IsThumbnail) ?? images.FirstOrDefault();
        var mainPath = main?.ImagePath ?? relic.ThumbnailPath;
        var inset = images.FirstOrDefault(i => !i.IsThumbnail && i.ImagePath != mainPath);

        return new HeroSlideVM
        {
            Name = relic.Name,
            Slug = relic.Slug,
            ProvinceName = relic.Province?.Name ?? string.Empty,
            TypeName = relic.RelicType?.Name ?? string.Empty,
            RankingLevel = relic.RankingLevel,
            RankingLabel = RankingLevelText.Label(relic.RankingLevel),
            RecognizedYear = relic.RecognizedYear,
            Intro = RelicIntroText.FirstSentence(relic.Description),
            MainImagePath = string.IsNullOrWhiteSpace(mainPath) ? "/img/no-image.svg" : mainPath,
            MainImageCaption = main?.Caption,
            InsetImagePath = inset?.ImagePath,
            InsetImageCaption = inset?.Caption
        };
    }
}
