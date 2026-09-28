using DiTichVietNam.Web.Services.Images;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public static class UploadReportTempData
{
    private const string SavedKey = "ImageUploadSaved";
    private const string NameKey = "ImageUploadRejectedNames";
    private const string ReasonKey = "ImageUploadRejectedReasons";

    public static void Save(ITempDataDictionary tempData, ImageUploadReport report)
    {
        if (report.SavedCount == 0 && !report.HasRejections)
        {
            return;
        }

        tempData[SavedKey] = report.SavedCount;

        if (!report.HasRejections)
        {
            return;
        }

        tempData[NameKey] = report.Rejections.Select(r => r.FileName).ToArray();
        tempData[ReasonKey] = report.Rejections.Select(r => r.Reason).ToArray();
    }

    public static ImageUploadReport? Read(ITempDataDictionary tempData)
    {
        if (tempData[SavedKey] is not int savedCount)
        {
            return null;
        }

        var report = new ImageUploadReport { SavedCount = savedCount };

        if (tempData[NameKey] is not string[] names || tempData[ReasonKey] is not string[] reasons)
        {
            return report;
        }

        for (var index = 0; index < names.Length && index < reasons.Length; index++)
        {
            report.Rejections.Add(new ImageUploadRejection
            {
                FileName = names[index],
                Reason = reasons[index]
            });
        }

        return report;
    }
}
