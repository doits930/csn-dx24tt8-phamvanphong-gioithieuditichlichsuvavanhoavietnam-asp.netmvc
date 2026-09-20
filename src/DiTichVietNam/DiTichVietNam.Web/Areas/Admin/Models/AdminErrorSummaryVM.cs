using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DiTichVietNam.Web.Areas.Admin.Models;

public class AdminErrorSummaryVM
{
    public string Title { get; set; } = "Biểu mẫu còn chỗ chưa hợp lệ";
    public List<AdminErrorLinkVM> Items { get; set; } = new();

    public bool HasItems => Items.Count > 0;

    public static AdminErrorSummaryVM FromModelState(
        ModelStateDictionary modelState,
        IReadOnlyList<(string Field, string TargetId)> fieldOrder)
    {
        var summary = new AdminErrorSummaryVM();

        foreach (var entry in modelState)
        {
            if (entry.Key.Length > 0 || entry.Value.Errors.Count == 0)
            {
                continue;
            }

            summary.Items.Add(new AdminErrorLinkVM { Message = entry.Value.Errors[0].ErrorMessage });
        }

        foreach (var (field, targetId) in fieldOrder)
        {
            var errors = modelState[field]?.Errors;
            if (errors is null || errors.Count == 0)
            {
                continue;
            }

            summary.Items.Add(new AdminErrorLinkVM
            {
                TargetId = targetId,
                Message = errors[0].ErrorMessage
            });
        }

        return summary;
    }
}

public class AdminErrorLinkVM
{
    public string? TargetId { get; set; }
    public string Message { get; set; } = string.Empty;
}
