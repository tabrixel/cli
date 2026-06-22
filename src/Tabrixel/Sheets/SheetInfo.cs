namespace Tabrixel.Sheets;

/// <summary>RowCount/ColumnCount describe the sheet grid size, not the number of records.</summary>
public sealed record SheetInfo(
    int SheetId,
    string Title,
    int RowCount,
    int ColumnCount);

public sealed record SpreadsheetMetadata(IReadOnlyList<SheetInfo> Sheets);
