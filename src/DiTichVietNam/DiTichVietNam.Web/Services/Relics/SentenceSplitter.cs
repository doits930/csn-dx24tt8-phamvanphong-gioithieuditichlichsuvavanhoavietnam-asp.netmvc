using System.Text;

namespace DiTichVietNam.Web.Services.Relics;

public static class SentenceSplitter
{
    private const int MinLeadLength = 30;

    private static readonly char[] Enders = { '.', '!', '?', '…' };

    private static readonly string[] Abbreviations =
    {
        "Tp", "TP", "Tk", "TK", "Th", "St", "GS", "Gs", "TS", "Ts", "PGS", "KTS", "ThS", "Ths",
        "NXB", "Nxb", "PTS", "BS", "KS"
    };

    public static List<string> Split(string? text)
    {
        var sentences = new List<string>();
        if (string.IsNullOrWhiteSpace(text))
        {
            return sentences;
        }

        var source = text!;
        var start = 0;

        foreach (var end in Boundaries(source, 0))
        {
            Add(sentences, source[start..end]);
            start = end;
        }

        if (start < source.Length)
        {
            Add(sentences, source[start..]);
        }

        return sentences;
    }

    public static int FirstSentenceEnd(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        foreach (var end in Boundaries(text!, MinLeadLength))
        {
            return end;
        }

        return text!.Length;
    }

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value!.Length);
        var previousWasSpace = false;

        foreach (var character in value.Trim())
        {
            if (char.IsWhiteSpace(character))
            {
                if (!previousWasSpace)
                {
                    builder.Append(' ');
                }

                previousWasSpace = true;
                continue;
            }

            builder.Append(character);
            previousWasSpace = false;
        }

        return builder.ToString();
    }

    private static IEnumerable<int> Boundaries(string text, int minLength)
    {
        var start = 0;
        var index = 0;

        while (index < text.Length)
        {
            if (Array.IndexOf(Enders, text[index]) < 0)
            {
                index++;
                continue;
            }

            var last = index;
            while (last + 1 < text.Length && Array.IndexOf(Enders, text[last + 1]) >= 0)
            {
                last++;
            }

            if (IsBoundary(text, start, index, last, minLength))
            {
                start = last + 1;
                yield return start;
            }

            index = last + 1;
        }
    }

    private static bool IsBoundary(string text, int start, int first, int last, int minLength)
    {
        if (last + 1 < text.Length && !char.IsWhiteSpace(text[last + 1]))
        {
            return false;
        }

        if (last + 1 - start < minLength)
        {
            return false;
        }

        return text[first] != '.' || !EndsWithAbbreviation(text, first);
    }

    private static bool EndsWithAbbreviation(string text, int dotIndex)
    {
        if (dotIndex > 0 && char.IsAsciiDigit(text[dotIndex - 1]))
        {
            return false;
        }

        var start = dotIndex;
        while (start > 0 && char.IsLetter(text[start - 1]))
        {
            start--;
        }

        var word = text[start..dotIndex];
        if (word.Length == 0)
        {
            return false;
        }

        if (Array.IndexOf(Abbreviations, word) >= 0)
        {
            return true;
        }

        return word.Length == 1 && char.IsUpper(word[0]);
    }

    private static void Add(List<string> sentences, string piece)
    {
        var trimmed = Normalize(piece);
        if (trimmed.Length > 0)
        {
            sentences.Add(trimmed);
        }
    }
}
