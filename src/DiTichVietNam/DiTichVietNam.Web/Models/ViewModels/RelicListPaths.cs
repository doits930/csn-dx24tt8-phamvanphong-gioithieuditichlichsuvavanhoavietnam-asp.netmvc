namespace DiTichVietNam.Web.Models.ViewModels;

public static class RelicListPaths
{
    public const string AllRelics = "/di-tich";

    public static string ByProvince(string slug) => $"/tinh/{slug}";

    public static string ByType(string slug) => $"/loai/{slug}";
}
