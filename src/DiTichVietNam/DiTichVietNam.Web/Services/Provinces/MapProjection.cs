namespace DiTichVietNam.Web.Services.Provinces;

public static class MapProjection
{
    public const double OriginLongitude = 102.0;
    public const double TopLatitude = 23.5;
    public const double ReferenceCosine = 0.9612617;
    public const double Scale = 70.0;

    public const double MinLongitude = 102.0;
    public const double MaxLongitude = 115.6725;
    public const double MinLatitude = 8.1429;
    public const double MaxLatitude = 23.5;

    public static bool TryProject(double? latitude, double? longitude, out double x, out double y)
    {
        x = 0;
        y = 0;

        if (latitude is not double lat || longitude is not double lon)
        {
            return false;
        }

        if (lat < MinLatitude || lat > MaxLatitude || lon < MinLongitude || lon > MaxLongitude)
        {
            return false;
        }

        x = Math.Round((lon - OriginLongitude) * ReferenceCosine * Scale, 1);
        y = Math.Round((TopLatitude - lat) * Scale, 1);
        return true;
    }
}
