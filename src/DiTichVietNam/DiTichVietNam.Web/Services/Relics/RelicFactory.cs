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
}
