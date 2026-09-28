namespace DiTichVietNam.Web.Services.Accounts;

public class AdminSiteOptions
{
    public const string SectionName = "AdminSite";

    public int Port { get; set; } = 5113;

    public int PublicPort { get; set; } = 5112;
}
