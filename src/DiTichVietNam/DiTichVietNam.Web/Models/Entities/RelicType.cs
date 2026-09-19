namespace DiTichVietNam.Web.Models.Entities;

public class RelicType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Relic> Relics { get; set; } = new List<Relic>();
}
