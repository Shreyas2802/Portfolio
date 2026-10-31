namespace Portfolio.Application.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}

/// <summary>
/// Server-enforced pagination bounds. The spec is explicit that the
/// server must clamp page size itself - a client passing
/// ?pageSize=999999999 must not be able to force an unbounded query.
/// </summary>
public static class PaginationLimits
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 48;

    public static int ClampPageSize(int requested) =>
        requested <= 0 ? DefaultPageSize : Math.Min(requested, MaxPageSize);

    public static int ClampPage(int requested) => requested <= 0 ? 1 : requested;
}
