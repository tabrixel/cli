using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Sheet description for describe: either columns and record count, or a header
/// error (a sheet with a broken header does not fail the whole command).
/// </summary>
public sealed record SheetDescription(
    string Name,
    IReadOnlyList<string>? Columns,
    int? Records,
    CliError? Error)
{
    public bool IsValid => Error is null;
}

public static class SheetDescriber
{
    public static SheetDescription Describe(
        string sheetName, IReadOnlyList<IReadOnlyList<string?>> values)
    {
        SheetHeader header;
        try
        {
            header = HeaderParser.Parse(values.Count > 0 ? values[0] : []);
        }
        catch (CliException ex) when (ex.Error.Code == ErrorCode.HeaderInvalid)
        {
            return new SheetDescription(sheetName, Columns: null, Records: null, ex.Error);
        }

        var columns = header.Columns.Select(c => c.Name).ToList();
        var records = Math.Max(0, values.Count - 1);
        return new SheetDescription(sheetName, columns, records, Error: null);
    }
}
