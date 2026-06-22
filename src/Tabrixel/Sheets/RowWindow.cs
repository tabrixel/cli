using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Applies <c>--offset</c> / <c>--limit</c> paging to an already-filtered list of
/// matched row indices. The caller captures the pre-paging count (the reported
/// <c>total</c>) before calling <see cref="Apply"/>, so <c>total</c> stays the
/// count of records matching <c>--where</c> before offset and limit. Ordering is
/// <c>--where</c> → <c>--offset</c> → <c>--limit</c>.
/// </summary>
public static class RowWindow
{
    /// <summary>
    /// Validates the paging arguments up front, before any network call, so bad
    /// input fails fast without requiring credentials. A negative offset or a
    /// non-positive limit is meaningless.
    /// </summary>
    public static void Validate(int offset, int limit)
    {
        if (offset < 0)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                $"--offset must be a non-negative integer, got {offset}");
        }

        if (limit < 1)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                $"--limit must be a positive integer, got {limit}");
        }
    }

    /// <summary>
    /// Skips the first <paramref name="offset"/> matched indices, then keeps at
    /// most <paramref name="limit"/>. An offset at or beyond the matched count
    /// yields an empty result. Assumes the arguments passed
    /// <see cref="Validate"/>.
    /// </summary>
    public static IReadOnlyList<int> Apply(IReadOnlyList<int> matched, int offset, int limit) =>
        matched.Skip(offset).Take(limit).ToList();
}
