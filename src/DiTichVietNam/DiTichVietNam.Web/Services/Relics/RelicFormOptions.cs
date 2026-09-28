namespace DiTichVietNam.Web.Services.Relics;

public class RelicFormOptions
{
    public List<RelicOptionGroup> ProvinceGroups { get; set; } = new();
    public List<RelicOptionItem> RelicTypes { get; set; } = new();
}

public class RelicOptionGroup
{
    public string Label { get; set; } = string.Empty;
    public List<RelicOptionItem> Items { get; set; } = new();
}

public class RelicOptionItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}
