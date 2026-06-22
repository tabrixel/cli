using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// A cell is empty when it has no significant characters (IsNullOrWhiteSpace);
/// significant names are taken as-is, without trimming.
/// </summary>
public static class HeaderParser
{
    public static SheetHeader Parse(IReadOnlyList<string?> firstRow)
    {
        var lastNonEmpty = LastNonEmptyIndex(firstRow);
        if (lastNonEmpty < 0)
        {
            throw new CliException(ErrorCode.HeaderInvalid,
                "header is invalid: the first row of the sheet is empty, no columns are defined");
        }

        // The empty tail to the right of the last non-empty cell is not columns.
        var cells = firstRow.Take(lastNonEmpty + 1).ToList();

        var holes = cells
            .Select((value, index) => (value, index))
            .Where(c => string.IsNullOrWhiteSpace(c.value))
            .Select(c => A1Notation.ColumnLetter(c.index))
            .ToList();
        if (holes.Count > 0)
        {
            throw new CliException(ErrorCode.HeaderInvalid,
                $"header is invalid: empty cell(s) in column(s) {string.Join(", ", holes)} " +
                "before the last non-empty header cell",
                new Dictionary<string, object?> { ["empty_columns"] = holes });
        }

        var columns = cells.Select((value, index) => new HeaderColumn(value!, index)).ToList();

        var duplicates = columns
            .GroupBy(c => c.Name, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .ToDictionary(
                g => g.Key,
                g => g.Select(c => A1Notation.ColumnLetter(c.Index)).ToList());
        if (duplicates.Count > 0)
        {
            var description = string.Join("; ", duplicates.Select(d =>
                $"'{d.Key}' in columns {string.Join(", ", d.Value)}"));
            throw new CliException(ErrorCode.HeaderInvalid,
                $"header is invalid: duplicate column name(s): {description}",
                new Dictionary<string, object?> { ["duplicates"] = duplicates });
        }

        return new SheetHeader(columns);
    }

    private static int LastNonEmptyIndex(IReadOnlyList<string?> cells)
    {
        for (var i = cells.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrWhiteSpace(cells[i]))
            {
                return i;
            }
        }

        return -1;
    }
}
