namespace Tabrixel.Sheets;

/// <summary>
/// One cell for a batch write: 0-based header column index, 1-based sheet row
/// number (header row is 1, first record is 2), new string value.
/// </summary>
public sealed record CellUpdate(int ColumnIndex, int RowNumber, string Value);
