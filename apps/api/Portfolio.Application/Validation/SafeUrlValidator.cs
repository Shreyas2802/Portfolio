namespace Portfolio.Application.Validation;

/// <summary>
/// Project GithubUrl/LiveUrl are rendered as real anchor hrefs on the
/// public site - an unrestricted string here would let something like
/// a "javascript:" URL through, which is a live XSS vector the moment
/// a visitor clicks it. Restricting to absolute http(s) URLs closes
/// that off without needing a general HTML sanitizer (the spec's
/// broader advice for rich text - not needed here since these are
/// plain link fields, not rendered as HTML).
/// </summary>
public static class SafeUrlValidator
{
    public static bool IsSafe(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return true; // both fields are optional - empty is fine
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
