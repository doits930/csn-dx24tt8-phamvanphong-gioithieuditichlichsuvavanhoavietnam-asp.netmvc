namespace DiTichVietNam.Web.Helpers;

public static class IntangibleHeritage
{
    public const string TypeSlug = "di-san-van-hoa-phi-vat-the";
    public const string TypeName = "Di sản văn hóa phi vật thể";

    public const string TypeDescription =
        "Tập quán, hình thức thể hiện, tri thức và kỹ năng được cộng đồng gìn giữ và trao truyền qua các thế hệ, "
        + "gồm tiếng nói chữ viết, ngữ văn dân gian, nghệ thuật trình diễn, tập quán xã hội và tín ngưỡng, lễ hội, "
        + "nghề thủ công và tri thức dân gian.";

    public const string AddressLabel = "Vùng thực hành";

    public const string AddressHint =
        "Ghi các tỉnh thành nơi di sản được thực hành, kèm tên đơn vị cũ khi đã sáp nhập.";

    public const string RankingFieldLabel = "Danh mục";
    public const string RecognizedYearFieldLabel = "Năm vào danh mục quốc gia";
    public const string TypeFieldLabel = "Loại hình";

    public const string PracticeSectionTitle = "Thời gian và nơi thực hành";
    public const string PracticeSectionShortLabel = "Thực hành";
    public const string UnescoChipLabel = "UNESCO ghi danh";
    public const string NationalListChipLabel = "Danh mục quốc gia";

    public const string ProvinceNote =
        "Với di sản văn hóa phi vật thể, tỉnh thành ghi kèm là địa phương tiêu biểu, "
        + "thực hành có thể trải trên nhiều tỉnh thành khác.";

    public const string IntroSectionTitle = "Giới thiệu về di sản";

    private const string AddressPrefix = "Vùng thực hành:";
    private const string PracticePrefix = "Thời gian và nơi thực hành:";
    private const string UnescoSentencePrefix = "Được UNESCO ghi danh năm";

    public static bool IsIntangible(string? typeSlug, string? typeName) =>
        string.Equals(typeSlug?.Trim(), TypeSlug, StringComparison.Ordinal) ||
        string.Equals(SlugHelper.ToSlug(typeName), TypeSlug, StringComparison.Ordinal);

    public static string EnsureAddressPrefix(string? address)
    {
        var text = address?.Trim() ?? string.Empty;
        if (text.Length == 0 || text.StartsWith(AddressPrefix, StringComparison.Ordinal))
        {
            return text;
        }

        return $"{AddressPrefix} {text}";
    }

    public static bool IsUnescoInscribed(string? description) =>
        description is not null &&
        description.TrimStart().StartsWith(UnescoSentencePrefix, StringComparison.Ordinal);

    public static string StripAddressPrefix(string? address) => StripPrefix(address, AddressPrefix);

    public static string StripPracticePrefix(string? line) => StripPrefix(line, PracticePrefix);

    public static string CapitalizeFirst(string? text)
    {
        var value = text ?? string.Empty;
        return value.Length == 0 ? value : char.ToUpperInvariant(value[0]) + value[1..];
    }

    private static string StripPrefix(string? value, string prefix)
    {
        var text = value?.Trim() ?? string.Empty;

        return text.StartsWith(prefix, StringComparison.Ordinal)
            ? text[prefix.Length..].TrimStart()
            : text;
    }
}
