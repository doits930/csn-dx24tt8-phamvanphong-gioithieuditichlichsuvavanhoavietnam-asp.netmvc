using System.Globalization;
using DiTichVietNam.Web.Areas.Admin.Validation;
using DiTichVietNam.Web.Services.Relics;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class RelicFormFactory
{
    public static RelicFormVM NewForm(RelicFormOptions options, string? returnUrl) => new()
    {
        Options = options,
        ReturnUrl = returnUrl
    };

    public static RelicFormVM ToFormVM(RelicEditData data, RelicFormOptions options, string? returnUrl) => new()
    {
        Id = data.Id,
        Name = data.Name,
        Slug = data.Slug,
        Address = data.Address,
        ProvinceId = data.ProvinceId.ToString(CultureInfo.InvariantCulture),
        RelicTypeId = data.RelicTypeId.ToString(CultureInfo.InvariantCulture),
        RankingLevel = ((int)data.RankingLevel).ToString(CultureInfo.InvariantCulture),
        RecognizedYear = data.RecognizedYear?.ToString(CultureInfo.InvariantCulture),
        Description = data.Description,
        History = data.History,
        VisitInfo = data.VisitInfo,
        SourceUrl = data.SourceUrl,
        Latitude = FormatCoordinate(data.Latitude),
        Longitude = FormatCoordinate(data.Longitude),
        ImageCount = data.ImageCount,
        Options = options,
        ReturnUrl = returnUrl
    };

    public static RelicInput ToInput(RelicFormVM vm)
    {
        FormNumberParser.TryParseWholeNumber(vm.ProvinceId, out var provinceId);
        FormNumberParser.TryParseWholeNumber(vm.RelicTypeId, out var relicTypeId);
        RelicFormValidator.TryReadRankingLevel(vm.RankingLevel, out var rankingLevel);

        int? recognizedYear = null;
        if (FormNumberParser.TryParseWholeNumber(vm.RecognizedYear, out var year))
        {
            recognizedYear = year;
        }

        double? latitude = null;
        double? longitude = null;
        if (FormNumberParser.TryParseDecimalNumber(vm.Latitude, out var latitudeValue)
            && FormNumberParser.TryParseDecimalNumber(vm.Longitude, out var longitudeValue))
        {
            latitude = latitudeValue;
            longitude = longitudeValue;
        }

        return new RelicInput
        {
            Name = vm.Name?.Trim() ?? string.Empty,
            Address = vm.Address?.Trim() ?? string.Empty,
            ProvinceId = provinceId,
            RelicTypeId = relicTypeId,
            RankingLevel = rankingLevel,
            RecognizedYear = recognizedYear,
            Description = vm.Description?.Trim() ?? string.Empty,
            History = vm.History,
            VisitInfo = vm.VisitInfo,
            SourceUrl = vm.SourceUrl?.Trim() ?? string.Empty,
            Latitude = latitude,
            Longitude = longitude
        };
    }

    private static string? FormatCoordinate(double? value) =>
        value?.ToString("0.######", CultureInfo.InvariantCulture);
}
