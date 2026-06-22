namespace Tabrixel.Sheets;

public static class WhereMatcher
{
    /// <summary>
    /// Ascending list of row indices in values (header is 0, records start at 1)
    /// matching ALL conditions (AND). Columns are resolved before iterating the
    /// rows, so a bad column name is not masked by an empty sheet. Cells
    /// normalize like in RecordReader (missing or null → ""); comparison is
    /// ordinal: case-sensitive, no type coercion, no trimming.
    /// </summary>
    public static IReadOnlyList<int> Match(
        SheetHeader header,
        IReadOnlyList<IReadOnlyList<string?>> values,
        IReadOnlyList<WhereCondition> conditions)
    {
        var resolved = conditions
            .Select(c => (header.FindColumn(c.ColumnName).Index, c.Value))
            .ToList();

        var matched = new List<int>();
        for (var i = 1; i < values.Count; i++)
        {
            var row = values[i];
            if (resolved.All(c => string.Equals(Cell(row, c.Index), c.Value, StringComparison.Ordinal)))
            {
                matched.Add(i);
            }
        }

        return matched;
    }

    private static string Cell(IReadOnlyList<string?> row, int index) =>
        index < row.Count ? row[index] ?? string.Empty : string.Empty;
}
