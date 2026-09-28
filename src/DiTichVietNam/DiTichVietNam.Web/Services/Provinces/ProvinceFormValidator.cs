using DiTichVietNam.Web.Helpers;

namespace DiTichVietNam.Web.Services.Provinces;

public static class ProvinceFormValidator
{
    public const int MaxNameLength = 100;

    public const string NameRequired = "Nhập tên tỉnh thành.";
    public const string NameTooLong = "Tên tỉnh thành tối đa 100 ký tự.";
    public const string NameWithoutLetter = "Tên tỉnh thành phải có ít nhất một chữ cái hoặc chữ số.";
    public const string RegionRequired = "Chọn miền của tỉnh thành.";

    public static List<(string Field, string Message)> Validate(ProvinceInput input)
    {
        var errors = new List<(string Field, string Message)>();

        var name = input.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors.Add((nameof(input.Name), NameRequired));
        }
        else if (name.Length > MaxNameLength)
        {
            errors.Add((nameof(input.Name), NameTooLong));
        }
        else if (SlugHelper.ToSlug(name).Length == 0)
        {
            errors.Add((nameof(input.Name), NameWithoutLetter));
        }

        if (!RegionText.IsKnown(input.Region))
        {
            errors.Add((nameof(input.Region), RegionRequired));
        }

        return errors;
    }
}
