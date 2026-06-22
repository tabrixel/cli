using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

public static class SheetResolver
{
    public static SheetInfo Resolve(SpreadsheetMetadata metadata, string? sheetName)
    {
        var sheets = metadata.Sheets;

        if (sheetName is not null)
        {
            // Sheet names are case-sensitive, like column names.
            var sheet = sheets.FirstOrDefault(s =>
                string.Equals(s.Title, sheetName, StringComparison.Ordinal));

            return sheet ?? throw new CliException(ErrorCode.NotFound,
                $"sheet '{sheetName}' not found; available sheets: {FormatTitles(sheets)}",
                AvailableSheetsDetails(sheets));
        }

        if (sheets.Count == 1)
        {
            return sheets[0];
        }

        throw new CliException(ErrorCode.SheetAmbiguous,
            $"the document has {sheets.Count} sheets; specify one with --sheet " +
            $"(available sheets: {FormatTitles(sheets)})",
            AvailableSheetsDetails(sheets));
    }

    private static string FormatTitles(IReadOnlyList<SheetInfo> sheets) =>
        string.Join(", ", sheets.Select(s => $"'{s.Title}'"));

    private static Dictionary<string, object?> AvailableSheetsDetails(IReadOnlyList<SheetInfo> sheets) =>
        new() { ["available_sheets"] = sheets.Select(s => s.Title).ToList() };
}
