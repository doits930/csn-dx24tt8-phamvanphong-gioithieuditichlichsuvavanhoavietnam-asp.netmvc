using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.Services.Accounts;

public static class AdminSiteStartupCheck
{
    private static readonly string[] OpenHosts = { "*", "+", "0.0.0.0", "[::]" };

    public static void Register(WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<AdminSiteOptions>>().Value;
        var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(AdminSiteStartupCheck));

        app.Lifetime.ApplicationStarted.Register(() => Inspect(app.Urls, options, logger));
    }

    private static void Inspect(ICollection<string> urls, AdminSiteOptions options, ILogger logger)
    {
        var adminUrls = urls
            .Select(ParseAddress)
            .Where(address => address.Port == options.Port)
            .ToList();

        if (adminUrls.Count == 0)
        {
            logger.LogWarning(
                "Ứng dụng không lắng nghe cổng quản trị {Port}, nên khu quản trị và trang đăng nhập sẽ không truy cập được. Thêm địa chỉ http://localhost:{Port} vào danh sách địa chỉ lắng nghe hoặc chỉnh mục AdminSite trong cấu hình.",
                options.Port,
                options.Port);
            return;
        }

        if (adminUrls.Any(address => OpenHosts.Contains(address.Host)))
        {
            logger.LogWarning(
                "Cổng quản trị {Port} đang mở cho mọi địa chỉ mạng. Nên chỉ lắng nghe trên localhost để máy khác không kết nối được tới khu quản trị.",
                options.Port);
        }
    }

    private static (string Host, int Port) ParseAddress(string url)
    {
        var schemeEnd = url.IndexOf("://", StringComparison.Ordinal);
        var authority = schemeEnd >= 0 ? url[(schemeEnd + 3)..] : url;
        var pathStart = authority.IndexOf('/');
        if (pathStart >= 0)
        {
            authority = authority[..pathStart];
        }

        var portStart = authority.LastIndexOf(':');
        if (portStart < 0 || !int.TryParse(authority[(portStart + 1)..], out var port))
        {
            return (authority, 0);
        }

        return (authority[..portStart], port);
    }
}
