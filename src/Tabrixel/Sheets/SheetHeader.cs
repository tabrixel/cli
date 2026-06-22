using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>Header cell name (as-is, no normalization) and 0-based sheet column index.</summary>
public sealed record HeaderColumn(string Name, int Index);

/// <summary>
/// A valid sheet header: constructed only by HeaderParser, so an invalid header
/// cannot exist as an instance of this type.
/// </summary>
public sealed class SheetHeader
{
    private readonly Dictionary<string, HeaderColumn> _byName;

    internal SheetHeader(IReadOnlyList<HeaderColumn> columns)
    {
        Columns = columns;
        _byName = columns.ToDictionary(c => c.Name, StringComparer.Ordinal);
    }

    public IReadOnlyList<HeaderColumn> Columns { get; }

    /// <summary>
    /// Subset of the header in the given order; the result is a regular
    /// SheetHeader, so record building and table rendering need no projection
    /// logic of their own. Unknown names fail like any other column lookup.
    /// </summary>
    public SheetHeader Project(IReadOnlyList<string> names)
    {
        return new SheetHeader(names.Select(FindColumn).ToList());
    }

    /// <summary>
    /// Exact (case-sensitive) lookup shared by all column-name consumers
    /// (--where, --json, --set). A miss is ColumnNotFound with a did_you_mean
    /// suggestion when a close name exists.
    /// </summary>
    public HeaderColumn FindColumn(string name)
    {
        if (_byName.TryGetValue(name, out var column))
        {
            return column;
        }

        var suggestion = SuggestColumn(name);
        var details = new Dictionary<string, object?>();
        var message = $"column '{name}' not found";
        if (suggestion is not null)
        {
            message += $"; did you mean '{suggestion}'?";
            details["did_you_mean"] = suggestion;
        }

        throw new CliException(ErrorCode.ColumnNotFound, message, details);
    }

    /// <summary>
    /// Case-insensitive match first, then the closest name by OSA distance within
    /// max(1, len/3) so clearly unrelated names are not suggested.
    /// </summary>
    private string? SuggestColumn(string name)
    {
        var caseInsensitive = Columns.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (caseInsensitive is not null)
        {
            return caseInsensitive.Name;
        }

        var threshold = Math.Max(1, name.Length / 3);
        var closest = Columns
            .Select(c => (c.Name, Distance: OsaDistance(name, c.Name)))
            .Where(c => c.Distance <= threshold)
            .OrderBy(c => c.Distance)
            .FirstOrDefault();

        return closest.Name;
    }

    /// <summary>
    /// Optimal String Alignment distance: Levenshtein plus adjacent-character
    /// transposition at cost 1, so the most common typo class stays within the
    /// suggestion threshold for short names.
    /// </summary>
    private static int OsaDistance(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;

        var prevPrev = new int[b.Length + 1];
        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var substitution = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + substitution);
                if (i > 1 && j > 1 && a[i - 1] == b[j - 2] && a[i - 2] == b[j - 1])
                {
                    current[j] = Math.Min(current[j], prevPrev[j - 2] + 1);
                }
            }

            (prevPrev, previous, current) = (previous, current, prevPrev);
        }

        return previous[b.Length];
    }
}
