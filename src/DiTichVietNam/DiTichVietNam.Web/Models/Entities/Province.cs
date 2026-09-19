namespace DiTichVietNam.Web.Models.Entities;

public class Province
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;

    public ICollection<Relic> Relics { get; set; } = new List<Relic>();
}
