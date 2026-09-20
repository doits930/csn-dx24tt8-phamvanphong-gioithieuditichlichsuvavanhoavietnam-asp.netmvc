namespace DiTichVietNam.Web.Services.RelicTypes;

public class RelicTypeAdminRow
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RelicCount { get; set; }
}

public class RelicTypeAdminSummary
{
    public int TotalCount { get; set; }
    public int WithRelicCount { get; set; }
    public int WithoutRelicCount { get; set; }
    public int RelicTotal { get; set; }
}

public class RelicTypeAdminListResult
{
    public List<RelicTypeAdminRow> Rows { get; set; } = new();
    public RelicTypeAdminSummary Summary { get; set; } = new();
}

public class RelicTypeEditData
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int RelicCount { get; set; }
}
