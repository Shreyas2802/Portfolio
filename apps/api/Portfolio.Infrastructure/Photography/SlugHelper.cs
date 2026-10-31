using System.Text.RegularExpressions;

namespace Portfolio.Infrastructure.Photography;

internal static partial class SlugHelper
{
    public static string Slugify(string input)
    {
        var lower = input.Trim().ToLowerInvariant();
        var withDashes = NonAlphanumeric().Replace(lower, "-");
        var collapsed = MultipleDashes().Replace(withDashes, "-");
        return collapsed.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultipleDashes();
}
