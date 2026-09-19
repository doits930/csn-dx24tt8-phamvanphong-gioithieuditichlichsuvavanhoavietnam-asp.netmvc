using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Helpers;

public static class RankingLevelText
{
    public static string Label(RankingLevel level) => level switch
    {
        RankingLevel.Provincial => "Di tích cấp tỉnh",
        RankingLevel.National => "Di tích quốc gia",
        RankingLevel.SpecialNational => "Di tích quốc gia đặc biệt",
        _ => "Chưa xếp hạng"
    };

    public static string ShortLabel(RankingLevel level) => level switch
    {
        RankingLevel.Provincial => "Cấp tỉnh",
        RankingLevel.National => "Quốc gia",
        RankingLevel.SpecialNational => "Quốc gia đặc biệt",
        _ => "Chưa xếp hạng"
    };

    public static string SummaryPhrase(RankingLevel level) => level switch
    {
        RankingLevel.Provincial => "cấp tỉnh",
        RankingLevel.National => "cấp quốc gia",
        RankingLevel.SpecialNational => "cấp quốc gia đặc biệt",
        _ => "chưa xếp hạng"
    };

    public static string CssModifier(RankingLevel level) => level switch
    {
        RankingLevel.Provincial => "ranking-provincial",
        RankingLevel.National => "ranking-national",
        RankingLevel.SpecialNational => "ranking-special",
        _ => "ranking-provincial"
    };
}
