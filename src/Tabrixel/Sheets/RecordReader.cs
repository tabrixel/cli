namespace Tabrixel.Sheets;

/// <summary>
/// Records are the rows below the header; the empty tail of the sheet never gets
/// here (values.get trims it), fully empty rows in the middle of the data count
/// as records (consistent with the SheetDescriber counter).
/// </summary>
public static class RecordReader
{
    /// <summary>
    /// Record keys are column names in column order (dictionary insertion order
    /// is preserved on serialization). A missing (row shorter than the header)
    /// or null cell reads as an empty string; cells beyond the last header
    /// column are ignored.
    /// </summary>
    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Read(
        SheetHeader header,
        IReadOnlyList<IReadOnlyList<string?>> values,
        int? limit = null)
    {
        var count = Math.Max(0, values.Count - 1);
        if (limit is { } max)
        {
            count = Math.Min(count, Math.Max(0, max));
        }

        var records = new List<IReadOnlyDictionary<string, string>>(count);
        for (var i = 1; i <= count; i++)
        {
            records.Add(ToRecord(header, values[i]));
        }

        return records;
    }

    /// <summary>
    /// Selection by row indices in values (the result of WhereMatcher.Match);
    /// records follow the order of the given indices.
    /// </summary>
    public static IReadOnlyList<IReadOnlyDictionary<string, string>> Read(
        SheetHeader header,
        IReadOnlyList<IReadOnlyList<string?>> values,
        IReadOnlyList<int> rowIndices)
    {
        var records = new List<IReadOnlyDictionary<string, string>>(rowIndices.Count);
        foreach (var index in rowIndices)
        {
            records.Add(ToRecord(header, values[index]));
        }

        return records;
    }

    private static Dictionary<string, string> ToRecord(SheetHeader header, IReadOnlyList<string?> row)
    {
        var record = new Dictionary<string, string>(header.Columns.Count);
        foreach (var column in header.Columns)
        {
            record[column.Name] =
                column.Index < row.Count ? row[column.Index] ?? string.Empty : string.Empty;
        }

        return record;
    }
}
