namespace DiTichVietNam.Web.Helpers;

public static class RegionText
{
    public static readonly string[] OrderedRegions = { "Bắc", "Trung", "Nam" };

    public static string DisplayName(string region) => $"Miền {region}";
}
