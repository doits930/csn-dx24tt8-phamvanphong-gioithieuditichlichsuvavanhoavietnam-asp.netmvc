using System.Text;

namespace DiTichVietNam.Web.Services.Relics;

public static class RelicIntroText
{
    private const int MaxLength = 160;
    private const int MinSentenceLength = 30;
    private static readonly char[] SentenceEnders = { '.', '!', '?', '…' };
    private static readonly string[] Abbreviations = { "Tp", "TP", "Tk", "TK", "Th", "St", "GS", "TS", "PGS", "KTS", "ThS", "Ths" };
    private const int MaxInitialLength = 2;

    public static string FirstSentence(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var text = CollapseWhitespace(description);
        if (text.Length == 0)
        {
            return string.Empty;
        }

        var sentence = TakeFirstSentence(text);
        return sentence.Length <= MaxLength ? sentence : Shorten(sentence);
    }

    private static string TakeFirstSentence(string text)
    {
        for (var i = 0; i < text.Length; i++)
        {
            if (Array.IndexOf(SentenceEnders, text[i]) < 0)
            {
                continue;
            }

            if (i + 1 < text.Length && !char.IsWhiteSpace(text[i + 1]))
            {
                continue;
            }

            if (i + 1 < MinSentenceLength)
            {
                continue;
            }

            if (text[i] == '.' && EndsWithAbbreviation(text, i))
            {
                continue;
            }

            return text[..(i + 1)];
        }

        return text;
    }

    private static string Shorten(string sentence)
    {
        var cut = sentence.LastIndexOf(' ', MaxLength - 1);
        if (cut <= 0)
        {
            cut = MaxLength - 1;
        }

        return sentence[..cut].TrimEnd(',', ';', ':', '-', ' ') + "…";
    }

    private static string CollapseWhitespace(string value)
    {
        var builder = new StringBuilder(value.Length);
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

    private static bool EndsWithAbbreviation(string text, int dotIndex)
    {
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

        return (word.Length <= MaxInitialLength && char.IsUpper(word[0]))
            || Array.IndexOf(Abbreviations, word) >= 0;
    }
}
