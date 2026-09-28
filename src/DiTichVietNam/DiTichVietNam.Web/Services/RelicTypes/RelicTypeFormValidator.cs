using DiTichVietNam.Web.Helpers;

namespace DiTichVietNam.Web.Services.RelicTypes;

public static class RelicTypeFormValidator
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 500;

    public const string NameRequired = "Nhập tên loại di tích.";
    public const string NameTooLong = "Tên loại di tích tối đa 100 ký tự.";
    public const string NameWithoutLetter = "Tên loại di tích phải có ít nhất một chữ cái hoặc chữ số.";
    public const string DescriptionTooLong = "Mô tả tối đa 500 ký tự.";

    public static List<(string Field, string Message)> Validate(RelicTypeInput input)
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

        if (input.Description?.Trim().Length > MaxDescriptionLength)
        {
            errors.Add((nameof(input.Description), DescriptionTooLong));
        }

        return errors;
    }
}
