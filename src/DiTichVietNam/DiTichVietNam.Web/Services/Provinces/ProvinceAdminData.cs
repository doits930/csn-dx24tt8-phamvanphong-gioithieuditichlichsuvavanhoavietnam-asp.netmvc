namespace DiTichVietNam.Web.Services.Provinces;

public class ProvinceAdminRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameNoAccent { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string RegionLabel { get; set; } = string.Empty;
    public int RelicCount { get; set; }
    public bool HasMapShape { get; set; }
}

public class ProvinceAdminSummary
{
    public int TotalCount { get; set; }
    public int WithRelicCount { get; set; }
    public int WithoutRelicCount { get; set; }
    public int WithoutMapCount { get; set; }
}

public class ProvinceAdminListResult
{
    public List<ProvinceAdminRow> Rows { get; set; } = new();
    public ProvinceAdminSummary Summary { get; set; } = new();
    public int MatchedCount { get; set; }
}

public class ProvinceEditData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public int RelicCount { get; set; }
    public bool HasMapShape { get; set; }
}
