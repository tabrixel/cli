using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Parsed --set assignment 'Column=value' for rows update: same syntax as
/// WhereCondition; an empty value clears the cell. The column name is not
/// validated here — SheetHeader.FindColumn does that.
/// </summary>
public sealed record SetAssignment(string ColumnName, string Value)
{
    public static SetAssignment Parse(string raw)
    {
        var separator = raw.IndexOf('=');
        if (separator < 1)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                $"invalid --set assignment '{raw}': expected format 'Column=value'");
        }

        return new SetAssignment(raw[..separator], raw[(separator + 1)..]);
    }
}
