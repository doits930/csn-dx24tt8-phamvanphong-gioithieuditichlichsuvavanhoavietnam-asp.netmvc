namespace DiTichVietNam.Web.Helpers;

public static class SiteUrls
{
    public static string OnPort(HttpRequest request, int port, string pathAndQuery)
    {
        var path = string.IsNullOrEmpty(pathAndQuery) ? "/" : pathAndQuery;
        return $"{request.Scheme}://{request.Host.Host}:{port}{path}";
    }
}
