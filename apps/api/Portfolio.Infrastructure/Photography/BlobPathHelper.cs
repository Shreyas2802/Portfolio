namespace Portfolio.Infrastructure.Photography;

/// <summary>
/// Pure string logic for mapping between storage-relative paths, blob
/// names and public URLs. Kept free of the Azure SDK so it can be unit
/// tested without a storage account or emulator.
/// </summary>
internal static class BlobPathHelper
{
    /// <summary>URL prefix used by the local-disk provider (served by the API's static files).</summary>
    public const string LocalUrlPrefix = "/uploads/";

    /// <summary>
    /// Turns a storage-relative path ("photos/slug/large.jpg", possibly
    /// with backslashes) into a blob name. Rejects empty segments and
    /// "." / ".." so a caller can never address a blob outside the
    /// intended folder structure.
    /// </summary>
    public static string ToBlobName(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var segments = relativePath.Replace('\\', '/').Trim('/').Split('/');
        foreach (var segment in segments)
        {
            if (segment.Length == 0 || segment == "." || segment == "..")
            {
                throw new InvalidOperationException($"Invalid storage path: '{relativePath}'.");
            }
        }

        return string.Join('/', segments);
    }

    /// <summary>Builds the public URL for a blob: base URL + URL-escaped blob name.</summary>
    public static string ToPublicUrl(string baseUrl, string blobName)
    {
        var escaped = string.Join('/', blobName.Split('/').Select(segment => Uri.EscapeDataString(segment)));
        return baseUrl.TrimEnd('/') + "/" + escaped;
    }

    /// <summary>
    /// Recovers the blob name from a URL produced by <see cref="ToPublicUrl"/>
    /// against any of the given base URLs. Returns false for URLs that
    /// don't belong to this storage (e.g. legacy "/uploads/..." paths).
    /// </summary>
    public static bool TryGetBlobName(string url, IEnumerable<string> baseUrls, out string blobName)
    {
        blobName = string.Empty;
        if (string.IsNullOrWhiteSpace(url)) return false;

        foreach (var baseUrl in baseUrls)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) continue;

            var prefix = baseUrl.TrimEnd('/') + "/";
            if (!url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;

            var remainder = url[prefix.Length..];
            var queryStart = remainder.IndexOfAny(['?', '#']);
            if (queryStart >= 0) remainder = remainder[..queryStart];

            try
            {
                blobName = ToBlobName(Uri.UnescapeDataString(remainder));
                return true;
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>True for paths written by the local-disk provider ("/uploads/...").</summary>
    public static bool IsLocalPath(string url) =>
        url.StartsWith(LocalUrlPrefix, StringComparison.OrdinalIgnoreCase);
}
