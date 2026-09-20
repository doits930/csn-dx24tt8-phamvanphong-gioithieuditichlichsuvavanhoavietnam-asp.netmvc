namespace DiTichVietNam.Web.Models.ViewModels;

public class VisitCardVM
{
    public List<string> TicketLines { get; set; } = new();
    public List<string> FreeLines { get; set; } = new();
    public List<string> TicketPendingLines { get; set; } = new();
    public List<string> HourLines { get; set; } = new();
    public List<string> NoteLines { get; set; } = new();
    public string? HeadlineAmount { get; set; }

    public bool ShowsTicket => TicketLines.Count > 0;
    public bool IsFreeEntry => TicketLines.Count == 0 && FreeLines.Count > 0;
    public bool HasHeadlineAmount => !string.IsNullOrWhiteSpace(HeadlineAmount);

    public bool HasAnything =>
        TicketLines.Count > 0 || FreeLines.Count > 0 || TicketPendingLines.Count > 0 ||
        HourLines.Count > 0 || NoteLines.Count > 0;
}
