namespace DiTichVietNam.Web.Services.Relics;

public static class RelicIntroText
{
    public const int CardMaxLength = 160;
    public const int CoverMaxLength = 260;

    public static string FirstSentence(string? description) =>
        FirstSentence(description, CardMaxLength);

    public static string FirstSentence(string? description, int maxLength)
    {
        var sentence = LeadSentence(description);
        return sentence.Length <= maxLength ? sentence : Shorten(sentence, maxLength);
    }

    public static string LeadSentence(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return string.Empty;
        }

        var text = SentenceSplitter.Normalize(description);
        if (text.Length == 0)
        {
            return string.Empty;
        }

        var end = SentenceSplitter.FirstSentenceEnd(text);
        return end <= 0 ? text : text[..end].Trim();
    }

    public static string Shorten(string sentence, int maxLength)
    {
        var cut = sentence.LastIndexOf(' ', maxLength - 1);
        if (cut <= 0)
        {
            cut = maxLength - 1;
        }

        return sentence[..cut].TrimEnd(',', ';', ':', '-', ' ') + "…";
    }
}
