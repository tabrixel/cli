using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

public static class A1Notation
{
    /// <summary>
    /// 1-based row of the first cell of an API-returned A1 range
    /// (e.g. "'Sheet 1'!A12:C12" → 12). A '!' inside a sheet title is always
    /// quoted, so the last '!' is the title/cells separator.
    /// </summary>
    public static int ParseStartRow(string range)
    {
        var cells = range[(range.LastIndexOf('!') + 1)..];
        var start = 0;
        while (start < cells.Length && char.IsAsciiLetter(cells[start]))
        {
            start++;
        }

        var end = start;
        while (end < cells.Length && char.IsAsciiDigit(cells[end]))
        {
            end++;
        }

        var digits = cells.AsSpan(start..end);
        if (digits.Length == 0 || !int.TryParse(digits, out var row) || row < 1)
        {
            throw new CliException(ErrorCode.Internal,
                $"cannot parse a row number out of range '{range}'");
        }

        return row;
    }

    /// <summary>0 → A, 25 → Z, 26 → AA (bijective base-26).</summary>
    public static string ColumnLetter(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);

        var letters = string.Empty;
        var n = index;
        while (n >= 0)
        {
            letters = (char)('A' + n % 26) + letters;
            n = n / 26 - 1;
        }

        return letters;
    }

    public static string FirstRowRange(string sheetTitle) =>
        $"{QuoteTitle(sheetTitle)}!1:1";

    public static string SheetRange(string sheetTitle) => QuoteTitle(sheetTitle);

    /// <summary>
    /// columnIndex is the 0-based header column index; rowNumber is the 1-based
    /// sheet row number (header row is 1, first record is 2).
    /// </summary>
    public static string Cell(string sheetTitle, int columnIndex, int rowNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rowNumber, 1);
        return $"{QuoteTitle(sheetTitle)}!{ColumnLetter(columnIndex)}{rowNumber}";
    }

    /// <summary>Single quotes inside a sheet title are doubled in A1 notation.</summary>
    private static string QuoteTitle(string sheetTitle) =>
        $"'{sheetTitle.Replace("'", "''")}'";
}
