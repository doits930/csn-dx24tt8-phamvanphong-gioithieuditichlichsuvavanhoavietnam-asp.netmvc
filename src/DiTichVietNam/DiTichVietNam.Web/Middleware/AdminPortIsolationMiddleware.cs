using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Services.Accounts;
using Microsoft.Extensions.Options;

namespace DiTichVietNam.Web.Middleware;

public class AdminPortIsolationMiddleware
{
    public const string AdminAreaPath = "/admin";
    public const string AccountAreaPath = "/tai-khoan";
    public const string StatusPath = "/loi";

    private const string UnknownSegment = "khong-tim-thay";

    private static readonly string[] AssetPrefixes =
    {
        "/css", "/js", "/lib", "/fonts", "/img", "/uploads", "/favicon"
    };

    private readonly RequestDelegate _next;
    private readonly AdminSiteOptions _options;

    public AdminPortIsolationMiddleware(RequestDelegate next, IOptions<AdminSiteOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var onAdminPort = context.Connection.LocalPort == _options.Port;

        if (IsRestrictedArea(path))
        {
            if (!onAdminPort)
            {
                context.Request.Path = ToUnknownPath(path);
                context.Request.QueryString = QueryString.Empty;
            }
        }
        else if (onAdminPort && !IsServedOnAdminPort(path))
        {
            var target = SiteUrls.OnPort(
                context.Request,
                _options.PublicPort,
                context.Request.Path + context.Request.QueryString);

            context.Response.Redirect(target, permanent: false);
            return;
        }

        await _next(context);
    }

    private static PathString ToUnknownPath(PathString path)
    {
        var segmentCount = (path.Value ?? string.Empty)
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Length;

        var segments = Enumerable.Repeat(UnknownSegment, Math.Max(segmentCount, 1));
        return new PathString("/" + string.Join('/', segments));
    }

    private static bool IsRestrictedArea(PathString path)
        => path.StartsWithSegments(AdminAreaPath) || path.StartsWithSegments(AccountAreaPath);

    private static bool IsServedOnAdminPort(PathString path)
    {
        if (path.StartsWithSegments(StatusPath))
        {
            return true;
        }

        var value = path.Value;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        foreach (var prefix in AssetPrefixes)
        {
            if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
