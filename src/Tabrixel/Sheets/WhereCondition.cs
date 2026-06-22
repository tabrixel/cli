using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Parsed --where condition 'Column=value': name is everything before the first
/// '=', value is the whole remainder (may contain '=' and may be empty — an
/// empty value matches an empty cell). Quotes and spaces are literal. The column
/// name is not validated here — SheetHeader.FindColumn does that on matching.
/// </summary>
public sealed record WhereCondition(string ColumnName, string Value)
{
    /// <summary>
    /// No '=' or an empty name to its left is a flag syntax error
    /// (InvalidArguments), unlike a miss on an existing name (ColumnNotFound).
    /// </summary>
    public static WhereCondition Parse(string raw)
    {
        var separator = raw.IndexOf('=');
        if (separator < 1)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                $"invalid --where condition '{raw}': expected format 'Column=value'");
        }

        return new WhereCondition(raw[..separator], raw[(separator + 1)..]);
    }
}
