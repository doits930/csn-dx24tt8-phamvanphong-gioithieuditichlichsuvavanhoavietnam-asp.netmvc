using DiTichVietNam.Web.Areas.Admin.Models;
using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.Entities;

namespace DiTichVietNam.Web.Areas.Admin.Validation;

public static class RelicFormValidator
{
    public const int MaxNameLength = 200;
    public const int MaxAddressLength = 300;
    public const int MaxSourceUrlLength = 500;
    public const int MaxDescriptionLength = 20000;
    public const int MaxHistoryLength = 20000;
    public const int MaxVisitInfoLength = 4000;
    public const int MinRecognizedYear = 1000;

    public const string NameRequired = "Nhập tên di tích.";
    public const string NameTooLong = "Tên di tích tối đa 200 ký tự.";
    public const string NameWithoutLetter = "Tên di tích phải có ít nhất một chữ cái hoặc chữ số.";
    public const string AddressRequired = "Nhập địa chỉ của di tích.";
    public const string AddressTooLong = "Địa chỉ tối đa 300 ký tự.";
    public const string ProvinceRequired = "Chọn tỉnh thành.";
    public const string RelicTypeRequired = "Chọn loại di tích.";
    public const string RankingRequired = "Chọn cấp xếp hạng.";
    public const string RecognizedYearNotNumber = "Năm xếp hạng phải là một số, ví dụ 1962.";
    public const string DescriptionRequired = "Nhập phần mô tả di tích.";
    public const string DescriptionTooLong = "Mô tả tối đa 20000 ký tự.";
    public const string HistoryTooLong = "Lịch sử tối đa 20000 ký tự.";
    public const string VisitInfoTooLong = "Thông tin tham quan tối đa 4000 ký tự.";
    public const string SourceUrlRequired = "Nhập đường dẫn nguồn tham khảo.";
    public const string SourceUrlTooLong = "Đường dẫn nguồn tham khảo tối đa 500 ký tự.";
    public const string SourceUrlFormat = "Đường dẫn nguồn tham khảo phải bắt đầu bằng http:// hoặc https://.";
    public const string LatitudeNotNumber = "Vĩ độ phải là một số, ví dụ 21.0293.";
    public const string LatitudeRange = "Vĩ độ phải nằm trong khoảng từ -90 đến 90.";
    public const string LongitudeNotNumber = "Kinh độ phải là một số, ví dụ 105.8342.";
    public const string LongitudeRange = "Kinh độ phải nằm trong khoảng từ -180 đến 180.";
    public const string CoordinatePairRequired = "Nhập đủ cả vĩ độ và kinh độ, hoặc bỏ trống cả hai.";

    public static string RecognizedYearRangeMessage =>
        $"Năm xếp hạng phải từ {MinRecognizedYear} đến {DateTime.Now.Year}.";

    public static List<(string Field, string Message)> Validate(RelicFormVM vm)
    {
        var errors = new List<(string Field, string Message)>();

        var name = vm.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors.Add((nameof(vm.Name), NameRequired));
        }
        else if (name.Length > MaxNameLength)
        {
            errors.Add((nameof(vm.Name), NameTooLong));
        }
        else if (SlugHelper.ToSlug(name).Length == 0)
        {
            errors.Add((nameof(vm.Name), NameWithoutLetter));
        }

        var address = vm.Address?.Trim();
        if (string.IsNullOrEmpty(address))
        {
            errors.Add((nameof(vm.Address), AddressRequired));
        }
        else if (address.Length > MaxAddressLength)
        {
            errors.Add((nameof(vm.Address), AddressTooLong));
        }

        if (!FormNumberParser.TryParseWholeNumber(vm.ProvinceId, out var provinceId) || provinceId <= 0)
        {
            errors.Add((nameof(vm.ProvinceId), ProvinceRequired));
        }

        if (!FormNumberParser.TryParseWholeNumber(vm.RelicTypeId, out var relicTypeId) || relicTypeId <= 0)
        {
            errors.Add((nameof(vm.RelicTypeId), RelicTypeRequired));
        }

        if (!TryReadRankingLevel(vm.RankingLevel, out _))
        {
            errors.Add((nameof(vm.RankingLevel), RankingRequired));
        }

        if (!string.IsNullOrWhiteSpace(vm.RecognizedYear))
        {
            if (!FormNumberParser.TryParseWholeNumber(vm.RecognizedYear, out var year))
            {
                errors.Add((nameof(vm.RecognizedYear), RecognizedYearNotNumber));
            }
            else if (year < MinRecognizedYear || year > DateTime.Now.Year)
            {
                errors.Add((nameof(vm.RecognizedYear), RecognizedYearRangeMessage));
            }
        }

        var description = vm.Description?.Trim();
        if (string.IsNullOrEmpty(description))
        {
            errors.Add((nameof(vm.Description), DescriptionRequired));
        }
        else if (description.Length > MaxDescriptionLength)
        {
            errors.Add((nameof(vm.Description), DescriptionTooLong));
        }

        if (vm.History?.Trim().Length > MaxHistoryLength)
        {
            errors.Add((nameof(vm.History), HistoryTooLong));
        }

        if (vm.VisitInfo?.Trim().Length > MaxVisitInfoLength)
        {
            errors.Add((nameof(vm.VisitInfo), VisitInfoTooLong));
        }

        var sourceUrl = vm.SourceUrl?.Trim();
        if (string.IsNullOrEmpty(sourceUrl))
        {
            errors.Add((nameof(vm.SourceUrl), SourceUrlRequired));
        }
        else if (sourceUrl.Length > MaxSourceUrlLength)
        {
            errors.Add((nameof(vm.SourceUrl), SourceUrlTooLong));
        }
        else if (!IsWebAddress(sourceUrl))
        {
            errors.Add((nameof(vm.SourceUrl), SourceUrlFormat));
        }

        AddCoordinateErrors(vm, errors);

        return errors;
    }

    public static bool TryReadRankingLevel(string? text, out RankingLevel level)
    {
        level = default;
        if (!FormNumberParser.TryParseWholeNumber(text, out var number))
        {
            return false;
        }

        if (!Enum.IsDefined(typeof(RankingLevel), number))
        {
            return false;
        }

        level = (RankingLevel)number;
        return true;
    }

    public static bool IsWebAddress(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static void AddCoordinateErrors(RelicFormVM vm, List<(string Field, string Message)> errors)
    {
        var hasLatitude = !string.IsNullOrWhiteSpace(vm.Latitude);
        var hasLongitude = !string.IsNullOrWhiteSpace(vm.Longitude);

        var latitudeValid = true;
        if (hasLatitude)
        {
            if (!FormNumberParser.TryParseDecimalNumber(vm.Latitude, out var latitude))
            {
                errors.Add((nameof(vm.Latitude), LatitudeNotNumber));
                latitudeValid = false;
            }
            else if (latitude < -90 || latitude > 90)
            {
                errors.Add((nameof(vm.Latitude), LatitudeRange));
                latitudeValid = false;
            }
        }

        var longitudeValid = true;
        if (hasLongitude)
        {
            if (!FormNumberParser.TryParseDecimalNumber(vm.Longitude, out var longitude))
            {
                errors.Add((nameof(vm.Longitude), LongitudeNotNumber));
                longitudeValid = false;
            }
            else if (longitude < -180 || longitude > 180)
            {
                errors.Add((nameof(vm.Longitude), LongitudeRange));
                longitudeValid = false;
            }
        }

        if (hasLatitude == hasLongitude || !latitudeValid || !longitudeValid)
        {
            return;
        }

        errors.Add((hasLatitude ? nameof(vm.Longitude) : nameof(vm.Latitude), CoordinatePairRequired));
    }
}
