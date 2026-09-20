using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public static class VisitInfoParser
{
    private const int MinHeadlineAmount = 1000;

    private static readonly string[] TicketWords =
    {
        "gia ve", "ve tham quan", "ve vao cua", "ve vao cong", "mien phi", "khong thu phi",
        "khong ban ve", "ban ve", "mua ve", "che do ve", "muc ve", "muc phi", "phi tham quan",
        "phi vao cua", "ve dien tu", "ve gop", "thu phi", "ve do", "ve thang may", "bang gia",
        "khong mat phi", "mo cua tu do", "vao cua tu do", "vao cong tu do", "khong thu ve",
        "khong thu tien", "muc gia"
    };

    private static readonly string[] TravelTicketWords =
    {
        "ve may bay", "ve tau hoa", "ve xe khach", "ve tau bay", "ve tau thuy"
    };

    private static readonly string[] FreeWords =
    {
        "khong ban ve", "khong thu phi", "khong thu ve", "khong mat phi", "khong thu tien",
        "mien phi vao cua", "mien phi vao cong", "mien phi tham quan", "mien phi hoan toan",
        "mo cua tu do", "vao cua tu do", "vao cong tu do"
    };

    private static readonly string[] ChargedWords =
    {
        "co ban ve", "co ve tham quan", "co ve vao cua", "ap dung che do ve", "ban ve tai",
        "ban ve tren", "thu phi", "thu ve", "mua ve", "ban ve", "co ve"
    };

    private static readonly string[] OpeningWords = { "mo cua", "dong cua", "phuc vu tu" };

    private static readonly string[] ScheduleWords =
    {
        "hang ngay", "quanh nam", "tu do", "thu hai", "thu ba", "thu tu", "thu nam",
        "thu sau", "thu bay", "chu nhat", "buoi sang", "buoi chieu", "sang va chieu"
    };

    private static readonly string[] TrafficWords = { "han che xe", "xe co gioi" };

    private static readonly string[] ContinuationWords =
    {
        "rieng ", "ngoai ra", "trong do", "ben canh do", "tuy nhien"
    };

    private static readonly string[] BackReferenceWords =
    {
        "khung gio nay", "gio nay", "muc gia nay", "gia nay", "muc phi nay", "muc ve nay",
        "quy dinh nay", "thoi gian nay", "khung gio tren", "muc gia tren"
    };

    public static VisitCardVM? Parse(string? visitInfo)
    {
        var sentences = SentenceSplitter.Split(visitInfo);
        if (sentences.Count == 0)
        {
            return null;
        }

        var card = new VisitCardVM();
        var previousGroup = VisitGroup.Note;
        var hasPrevious = false;

        foreach (var sentence in sentences)
        {
            var folded = SlugHelper.FoldToAsciiLower(sentence);
            var state = ReadTicketState(sentence, folded);
            var group = ChooseGroup(folded, state);

            if (state == TicketState.Unknown && hasPrevious && IsContinuation(folded))
            {
                group = previousGroup;
            }

            switch (group)
            {
                case VisitGroup.Ticket when state == TicketState.Charged:
                    card.TicketLines.Add(sentence);
                    break;
                case VisitGroup.Ticket when state == TicketState.Free:
                    card.FreeLines.Add(sentence);
                    break;
                case VisitGroup.Ticket:
                    card.TicketPendingLines.Add(sentence);
                    break;
                case VisitGroup.Hours:
                    card.HourLines.Add(sentence);
                    break;
                default:
                    card.NoteLines.Add(sentence);
                    break;
            }

            previousGroup = group;
            hasPrevious = true;
        }

        card.HeadlineAmount = FindHeadlineAmount(card.TicketLines);
        return card;
    }

    private static VisitGroup ChooseGroup(string folded, TicketState state)
    {
        var isTicket = !ContainsAny(folded, TravelTicketWords) && ContainsAny(folded, TicketWords);

        if (isTicket && (state != TicketState.Unknown || !HasClockTime(folded)))
        {
            return VisitGroup.Ticket;
        }

        if (IsOpeningHours(folded))
        {
            return VisitGroup.Hours;
        }

        return isTicket ? VisitGroup.Ticket : VisitGroup.Note;
    }

    private static TicketState ReadTicketState(string sentence, string folded)
    {
        var hasZeroAmount = false;

        foreach (var amount in ReadAmounts(sentence, folded))
        {
            if (amount.Value >= MinHeadlineAmount)
            {
                return TicketState.Charged;
            }

            if (amount.Value == 0)
            {
                hasZeroAmount = true;
            }
        }

        if (hasZeroAmount || ContainsAny(folded, FreeWords))
        {
            return TicketState.Free;
        }

        return ContainsAnyAffirmed(folded, ChargedWords) ? TicketState.Charged : TicketState.Unknown;
    }

    private static bool IsContinuation(string folded)
    {
        foreach (var word in ContinuationWords)
        {
            if (folded.StartsWith(word, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return ContainsAny(folded, BackReferenceWords);
    }

    private static string? FindHeadlineAmount(List<string> ticketLines)
    {
        foreach (var sentence in ticketLines)
        {
            var folded = SlugHelper.FoldToAsciiLower(sentence);
            foreach (var amount in ReadAmounts(sentence, folded))
            {
                if (amount.Value >= MinHeadlineAmount)
                {
                    return $"{amount.Text} đồng";
                }
            }
        }

        return null;
    }

    private static bool IsOpeningHours(string folded)
    {
        if (folded.Contains("gio mo cua"))
        {
            return true;
        }

        if (ContainsAny(folded, TrafficWords))
        {
            return false;
        }

        if (!ContainsAny(folded, OpeningWords))
        {
            return false;
        }

        return ContainsAny(folded, ScheduleWords) || HasClockTime(folded);
    }

    private static bool HasClockTime(string folded)
    {
        for (var index = 0; index < folded.Length; index++)
        {
            if (!char.IsAsciiDigit(folded[index]))
            {
                continue;
            }

            var end = index;
            while (end < folded.Length && char.IsAsciiDigit(folded[end]))
            {
                end++;
            }

            var rest = folded[end..].TrimStart();
            if (rest.StartsWith('h') || rest.StartsWith("gio"))
            {
                return true;
            }

            index = end;
        }

        return false;
    }

    private static IEnumerable<MoneyAmount> ReadAmounts(string sentence, string folded)
    {
        var index = 0;
        while (index < sentence.Length)
        {
            if (!char.IsAsciiDigit(sentence[index]))
            {
                index++;
                continue;
            }

            var end = index;
            while (end < sentence.Length &&
                   (char.IsAsciiDigit(sentence[end]) ||
                    (sentence[end] == '.' && end + 1 < sentence.Length && char.IsAsciiDigit(sentence[end + 1]))))
            {
                end++;
            }

            var text = sentence[index..end];
            var rest = folded[end..].TrimStart();
            if (rest.StartsWith("dong") && long.TryParse(text.Replace(".", string.Empty), out var value))
            {
                yield return new MoneyAmount(value, text);
            }

            index = end;
        }
    }

    private static bool ContainsAny(string folded, string[] words)
    {
        foreach (var word in words)
        {
            if (folded.Contains(word, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAnyAffirmed(string folded, string[] words)
    {
        foreach (var word in words)
        {
            var position = folded.IndexOf(word, StringComparison.Ordinal);
            while (position >= 0)
            {
                if (!IsNegated(folded, position))
                {
                    return true;
                }

                position = folded.IndexOf(word, position + 1, StringComparison.Ordinal);
            }
        }

        return false;
    }

    private static bool IsNegated(string folded, int position)
    {
        var before = folded[Math.Max(0, position - 12)..position].TrimEnd();
        return before.EndsWith("khong", StringComparison.Ordinal) ||
               before.EndsWith("chua", StringComparison.Ordinal) ||
               before.EndsWith("khong con", StringComparison.Ordinal);
    }

    private enum TicketState
    {
        Unknown,
        Free,
        Charged
    }

    private enum VisitGroup
    {
        Ticket,
        Hours,
        Note
    }

    private readonly record struct MoneyAmount(long Value, string Text);
}
