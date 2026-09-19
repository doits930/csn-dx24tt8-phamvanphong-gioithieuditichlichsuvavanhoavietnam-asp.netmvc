using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;
using DiTichVietNam.Web.Models.ViewModels;

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
