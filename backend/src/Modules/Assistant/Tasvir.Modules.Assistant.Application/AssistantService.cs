using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;

namespace Tasvir.Modules.Assistant.Application;

public sealed record AskRequest(string? Question);
public sealed record AssistantLink(string Label, string Href);
public sealed record AssistantAnswer(string Answer, bool IsMatched, AssistantLink? Link, IReadOnlyList<string> Suggestions);

/// <summary>
/// Javob manbai. Hozir bilim bazasi; keyinchalik til modeliga asoslangan amalga oshirish shu interfeys orqali ulanadi,
/// endpoint va frontend o‘zgarmaydi.
/// </summary>
public interface IAnswerProvider
{
    AssistantAnswer Answer(string question);
    IReadOnlyList<string> DefaultSuggestions { get; }
}

/// <summary>Savoldagi so‘zlarni bilim bazasi kalit so‘zlari bilan solishtiradi. Javob faqat bazadan olinadi, to‘qib chiqarilmaydi.</summary>
public sealed partial class KnowledgeBaseAnswerProvider : IAnswerProvider
{
    private const int StrongWeight = 2;
    private const int WeakWeight = 1;
    private const int MinScore = 2;

    private sealed record Entry(KnowledgeEntry Source, string[] Strong, string[] Weak);

    private readonly List<Entry> _entries = KnowledgeBase.Entries
        .Select(e => new Entry(e, e.Strong.Select(Normalize).ToArray(), e.Weak.Select(Normalize).ToArray()))
        .ToList();

    public IReadOnlyList<string> DefaultSuggestions { get; } = KnowledgeBase.DefaultSuggestions;

    public AssistantAnswer Answer(string question)
    {
        var tokens = Tokenize(question);

        var best = _entries
            .Select(entry => (Entry: entry, Score: Score(entry, tokens)))
            .Where(x => x.Score >= MinScore)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (best.Entry is null)
        {
            return new AssistantAnswer(KnowledgeBase.Fallback, false, null, DefaultSuggestions);
        }

        var source = best.Entry.Source;
        var related = source.Related.Count > 0 ? source.Related : DefaultSuggestions.Where(s => s != source.Question).ToList();
        return new AssistantAnswer(source.Answer, true, source.Link, related);
    }

    // O‘zbek tilida qo‘shimchalar so‘z oxiriga qo‘shiladi: "parol" kaliti "parolni", "parolim" ni ham topadi
    private static int Score(Entry entry, IReadOnlyList<string> tokens) =>
        entry.Strong.Count(key => tokens.Any(t => t.StartsWith(key, StringComparison.Ordinal))) * StrongWeight
        + entry.Weak.Count(key => tokens.Any(t => t.StartsWith(key, StringComparison.Ordinal))) * WeakWeight;

    private static List<string> Tokenize(string text) =>
        WordRegex().Matches(Normalize(text)).Select(m => m.Value).ToList();

    /// <summary>
    /// Yozuv farqlarini bir xil ko‘rinishga keltiradi: o‘/o'/oʻ/o` va yangi alifbodagi ö, ğ, ş, ç.
    /// </summary>
    public static string Normalize(string text)
    {
        var lower = text.ToLowerInvariant()
            .Replace("ö", "o").Replace("ğ", "g").Replace("ş", "sh").Replace("ç", "ch");
        return ApostropheRegex().Replace(lower, string.Empty);
    }

    [GeneratedRegex(@"[\p{L}\p{N}]+")]
    private static partial Regex WordRegex();

    [GeneratedRegex(@"['‘’ʻʼ`´]")]
    private static partial Regex ApostropheRegex();
}

public static class DependencyInjection
{
    public static IServiceCollection AddAssistantApplication(this IServiceCollection services)
    {
        services.AddSingleton<IAnswerProvider, KnowledgeBaseAnswerProvider>();
        return services;
    }
}
