using DiTichVietNam.Web.Helpers;
using DiTichVietNam.Web.Models.ViewModels;

namespace DiTichVietNam.Web.Services.Relics;

public static class RelicTimelineParser
{
    private const int MinEntries = 3;
    private const int MaxEntries = 6;
    private const int MaxSentenceLength = 320;
    private const int MinYear = 100;
    private const int MaxCentury = 21;
    private const int CenturySpan = 100;
    private const int DecadeSpan = 10;
    private const string DecadePhrase = "thap nien";

    private static readonly string[] CodePrefixes = { "so", "qd", "quyet dinh so", "nghi quyet so", "thong tu so" };

    private static readonly string[] MeasureWords =
    {
        "km", "kilomet", "ki-lo-met", "ki lo met", "met", "hecta", "tuoi", "nguoi", "luot",
        "tam", "vi", "gio", "phut", "cm", "phan tram", "hien vat", "bac", "dong", "ngoi",
        "chiec", "buc", "con", "trang", "ha ", "ha,", "ha.", "ha)"
    };

    public static List<TimelineEntryVM> Parse(string? history)
    {
        var sentences = SentenceSplitter.Split(history);
        var entries = new List<TimelineEntryVM>();

        foreach (var sentence in sentences)
        {
            if (sentence.Length > MaxSentenceLength)
            {
                continue;
            }

            var marks = FindMarks(sentence);
            if (marks.Count == 0)
            {
                continue;
            }

            var earliest = marks.OrderBy(mark => mark.SortKey).First();
            entries.Add(new TimelineEntryVM
            {
                SortKey = earliest.SortKey,
                YearLabel = earliest.Label,
                Sentence = sentence
            });
        }

        if (entries.Count < MinEntries)
        {
            return new List<TimelineEntryVM>();
        }

        var unique = new List<TimelineEntryVM>();
        var seenLabels = new HashSet<string>();
        foreach (var entry in entries.OrderBy(entry => entry.SortKey))
        {
            if (seenLabels.Add(entry.YearLabel))
            {
                unique.Add(entry);
            }
        }

        if (unique.Count < MinEntries)
        {
            return new List<TimelineEntryVM>();
        }

        return unique.Take(MaxEntries).ToList();
    }

    private static List<TimeMark> FindMarks(string sentence)
    {
        var folded = SlugHelper.FoldToAsciiLower(sentence);
        var marks = new List<TimeMark>();
        var index = 0;

        while (index < sentence.Length)
        {
            if (StartsCenturyPhrase(folded, index))
            {
                index = ReadCentury(sentence, folded, index, marks);
                continue;
            }

            if (char.IsAsciiDigit(sentence[index]))
            {
                index = ReadYear(sentence, folded, index, marks);
                continue;
            }

            index++;
        }

        return marks;
    }

    private static bool StartsCenturyPhrase(string folded, int index) =>
        string.CompareOrdinal(folded, index, "the ky", 0, 6) == 0 &&
        (index == 0 || !char.IsLetterOrDigit(folded[index - 1]));

    private static int ReadCentury(string sentence, string folded, int index, List<TimeMark> marks)
    {
        var cursor = index + 6;
        while (cursor < sentence.Length && sentence[cursor] == ' ')
        {
            cursor++;
        }

        var start = cursor;
        while (cursor < sentence.Length && RomanValue(char.ToUpperInvariant(sentence[cursor])) > 0)
        {
            cursor++;
        }

        var century = 0;
        if (cursor > start)
        {
            century = ReadRoman(sentence[start..cursor]);
        }
        else
        {
            while (cursor < sentence.Length && char.IsAsciiDigit(sentence[cursor]))
            {
                cursor++;
            }

            if (cursor > start)
            {
                century = int.Parse(sentence[start..cursor]);
            }
        }

        if (century >= 1 && century <= MaxCentury)
        {
            var beforeCommonEra = LooksBeforeCommonEra(folded, cursor);
            var offset = PeriodOffset(folded, index, CenturySpan);
            var start100 = (century - 1) * 100;
            var year = beforeCommonEra
                ? -(start100 + CenturySpan - offset)
                : start100 + offset;
            marks.Add(new TimeMark(
                year,
                $"Thế kỷ {sentence[start..cursor]}{(beforeCommonEra ? " TCN" : string.Empty)}"));
        }

        return cursor > index ? cursor : index + 1;
    }

    private static int ReadYear(string sentence, string folded, int index, List<TimeMark> marks)
    {
        var end = index;
        while (end < sentence.Length && char.IsAsciiDigit(sentence[end]))
        {
            end++;
        }

        var token = sentence[index..end];
        if (token.Length < 3 || !int.TryParse(token, out var value) || value < MinYear || value > DateTime.Now.Year)
        {
            return end;
        }

        if (index > 0 && (char.IsAsciiDigit(sentence[index - 1]) || "./-,".Contains(sentence[index - 1])))
        {
            return end;
        }

        if (end < sentence.Length && "./-".Contains(sentence[end]) &&
            end + 1 < sentence.Length && char.IsLetterOrDigit(sentence[end + 1]))
        {
            return end;
        }

        var before = folded[Math.Max(0, index - 24)..index].TrimEnd();
        foreach (var prefix in CodePrefixes)
        {
            if (before.EndsWith(prefix, StringComparison.Ordinal))
            {
                return end;
            }
        }

        var isDecade = before.EndsWith("thap nien", StringComparison.Ordinal);
        if (!isDecade && !before.EndsWith("nam", StringComparison.Ordinal))
        {
            return end;
        }

        var after = folded[end..Math.Min(folded.Length, end + 26)].TrimStart();
        foreach (var measure in MeasureWords)
        {
            if (after.StartsWith(measure.TrimEnd(), StringComparison.Ordinal))
            {
                return end;
            }
        }

        var beforeCommonEra = LooksBeforeCommonEra(folded, end);
        var offset = isDecade
            ? QualifierOffset(before[..^DecadePhrase.Length].TrimEnd(), DecadeSpan)
            : 0;
        var sortKey = beforeCommonEra
            ? -(value + (isDecade ? DecadeSpan - offset : 0))
            : value + offset;

        marks.Add(new TimeMark(sortKey, beforeCommonEra ? $"{token} TCN" : token));

        return end;
    }

    private static int PeriodOffset(string folded, int phraseIndex, int span) =>
        QualifierOffset(folded[Math.Max(0, phraseIndex - 16)..phraseIndex].TrimEnd(), span);

    private static int QualifierOffset(string before, int span)
    {
        if (before.EndsWith("nua dau", StringComparison.Ordinal))
        {
            return span / 4;
        }

        if (before.EndsWith("nua sau", StringComparison.Ordinal) ||
            before.EndsWith("nua cuoi", StringComparison.Ordinal))
        {
            return span * 3 / 4;
        }

        if (before.EndsWith("dau", StringComparison.Ordinal))
        {
            return span / 10;
        }

        if (before.EndsWith("giua", StringComparison.Ordinal))
        {
            return span / 2;
        }

        if (before.EndsWith("cuoi", StringComparison.Ordinal))
        {
            return span * 9 / 10;
        }

        return span / 2;
    }

    private static bool LooksBeforeCommonEra(string folded, int position)
    {
        var window = folded[Math.Min(position, folded.Length)..Math.Min(folded.Length, position + 26)];
        return window.Contains("truoc cong nguyen", StringComparison.Ordinal);
    }

    private static int ReadRoman(string token)
    {
        var total = 0;
        var previous = 0;

        for (var index = token.Length - 1; index >= 0; index--)
        {
            var value = RomanValue(char.ToUpperInvariant(token[index]));
            if (value == 0)
            {
                return 0;
            }

            if (value < previous)
            {
                total -= value;
            }
            else
            {
                total += value;
                previous = value;
            }
        }

        return total;
    }

    private static int RomanValue(char letter) => letter switch
    {
        'I' => 1,
        'V' => 5,
        'X' => 10,
        'L' => 50,
        'C' => 100,
        'D' => 500,
        'M' => 1000,
        _ => 0
    };

    private readonly record struct TimeMark(int SortKey, string Label);
}
