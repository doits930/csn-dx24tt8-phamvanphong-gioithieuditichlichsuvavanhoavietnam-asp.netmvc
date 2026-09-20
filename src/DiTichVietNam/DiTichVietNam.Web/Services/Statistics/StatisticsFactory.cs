using System.Globalization;

namespace DiTichVietNam.Web.Services.Statistics;

public static class StatisticsFactory
{
    public static void ApplyShares(List<CountShare> items, int total)
    {
        var max = 0;
        foreach (var item in items)
        {
            if (item.Count > max)
            {
                max = item.Count;
            }
        }

        foreach (var item in items)
        {
            item.Percent = Percent(item.Count, total);
            item.ShareWidth = Width(item.Count, total);
            item.BarWidth = Width(item.Count, max);
        }
    }

    public static int Percent(int count, int total)
        => total <= 0 || count <= 0 ? 0 : (int)Math.Round(count * 100d / total, MidpointRounding.AwayFromZero);

    public static string Width(int count, int reference)
    {
        if (reference <= 0 || count <= 0)
        {
            return "0";
        }

        var ratio = Math.Min(count * 100d / reference, 100d);

        return ratio.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
