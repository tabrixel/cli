using Google;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Tabrixel.Infrastructure;

namespace Tabrixel.Sheets;

/// <summary>
/// Per-CLI-invocation context for one document: lazily loads metadata (one API
/// request per invocation) and caches headers/values per sheet. Commands that
/// work with a sheet obtain it only from here.
/// </summary>
public sealed class SpreadsheetContext(SheetsClient client, string spreadsheetId)
{
    private SpreadsheetMetadata? _metadata;
    private readonly Dictionary<int, SheetHeader> _headers = [];
    private readonly Dictionary<int, IReadOnlyList<IReadOnlyList<string?>>> _values = [];

    public string SpreadsheetId => spreadsheetId;

    public SpreadsheetMetadata Metadata => _metadata ??= LoadMetadata();

    public SheetInfo ResolveSheet(string? sheetName) =>
        SheetResolver.Resolve(Metadata, sheetName);

    public SheetHeader LoadHeader(SheetInfo sheet)
    {
        if (_headers.TryGetValue(sheet.SheetId, out var cached))
        {
            return cached;
        }

        IList<object>? firstRow;
        try
        {
            var request = client.Service.Spreadsheets.Values.Get(
                spreadsheetId, A1Notation.FirstRowRange(sheet.Title));
            var response = request.Execute();
            firstRow = response.Values is { Count: > 0 } ? response.Values[0] : null;
        }
        catch (GoogleApiException ex)
        {
            throw ex.ToSpreadsheetAccessError(spreadsheetId, client.ServiceAccountEmail);
        }

        var cells = (firstRow ?? []).Select(v => v?.ToString()).ToList();
        var header = HeaderParser.Parse(cells);
        _headers[sheet.SheetId] = header;
        return header;
    }

    /// <summary>All sheet values via one values.get; the API trims the empty tail itself.</summary>
    public IReadOnlyList<IReadOnlyList<string?>> LoadValues(SheetInfo sheet)
    {
        if (_values.TryGetValue(sheet.SheetId, out var cached))
        {
            return cached;
        }

        IList<IList<object>>? rows;
        try
        {
            var request = client.Service.Spreadsheets.Values.Get(
                spreadsheetId, A1Notation.SheetRange(sheet.Title));
            var response = request.Execute();
            rows = response.Values;
        }
        catch (GoogleApiException ex)
        {
            throw ex.ToSpreadsheetAccessError(spreadsheetId, client.ServiceAccountEmail);
        }

        var values = (rows ?? [])
            .Select(IReadOnlyList<string?> (row) => row.Select(v => v?.ToString()).ToList())
            .ToList();
        _values[sheet.SheetId] = values;
        return values;
    }

    /// <summary>
    /// values.append finds the end of the data itself — the sheet is not read,
    /// so there is no race with parallel writers. RAW: values are written as-is;
    /// OVERWRITE: trailing empty grid rows are filled, not pushed down.
    /// Returns the 1-based sheet row the record landed in, taken from the
    /// response's updated range — the authoritative placement.
    /// </summary>
    public int AppendRow(SheetInfo sheet, IReadOnlyList<string> row)
    {
        string? updatedRange;
        try
        {
            var body = new ValueRange { Values = [row.Cast<object>().ToList()] };
            var request = client.Service.Spreadsheets.Values.Append(
                body, spreadsheetId, A1Notation.SheetRange(sheet.Title));
            request.ValueInputOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.RAW;
            request.InsertDataOption =
                SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.OVERWRITE;
            var response = request.Execute();
            updatedRange = response.Updates?.UpdatedRange;
        }
        catch (GoogleApiException ex)
        {
            throw ex.ToSpreadsheetAccessError(spreadsheetId, client.ServiceAccountEmail);
        }

        // The cached values no longer reflect the sheet content.
        _values.Remove(sheet.SheetId);

        if (updatedRange is null)
        {
            throw new CliException(ErrorCode.Internal,
                "append succeeded but the response carried no updated range");
        }

        return A1Notation.ParseStartRow(updatedRange);
    }

    /// <summary>
    /// Targeted write of changed cells in one values.batchUpdate request (RAW):
    /// only the given cells are touched. An empty list makes no API call.
    /// </summary>
    public void UpdateCells(SheetInfo sheet, IReadOnlyList<CellUpdate> updates)
    {
        if (updates.Count == 0)
        {
            return;
        }

        try
        {
            var body = new BatchUpdateValuesRequest
            {
                ValueInputOption = "RAW",
                Data = updates
                    .Select(u => new ValueRange
                    {
                        Range = A1Notation.Cell(sheet.Title, u.ColumnIndex, u.RowNumber),
                        Values = [[u.Value]],
                    })
                    .ToList(),
            };
            var request = client.Service.Spreadsheets.Values.BatchUpdate(body, spreadsheetId);
            request.Execute();
        }
        catch (GoogleApiException ex)
        {
            throw ex.ToSpreadsheetAccessError(spreadsheetId, client.ServiceAccountEmail);
        }

        _values.Remove(sheet.SheetId);
    }

    /// <summary>
    /// Whole-row deletion in one spreadsheets.batchUpdate (DeleteDimension by
    /// ROWS); ranges are built bottom-up (RowDeletion) so deleting one row does
    /// not shift the addressing of the rest. An empty list makes no API call.
    /// </summary>
    public void DeleteRows(SheetInfo sheet, IReadOnlyList<int> rowIndices)
    {
        var ranges = RowDeletion.ToRanges(rowIndices);
        if (ranges.Count == 0)
        {
            return;
        }

        try
        {
            var body = new BatchUpdateSpreadsheetRequest
            {
                Requests = ranges
                    .Select(r => new Request
                    {
                        DeleteDimension = new DeleteDimensionRequest
                        {
                            Range = new DimensionRange
                            {
                                SheetId = sheet.SheetId,
                                Dimension = "ROWS",
                                StartIndex = r.Start,
                                EndIndex = r.End,
                            },
                        },
                    })
                    .ToList(),
            };
            var request = client.Service.Spreadsheets.BatchUpdate(body, spreadsheetId);
            request.Execute();
        }
        catch (GoogleApiException ex)
        {
            throw ex.ToSpreadsheetAccessError(spreadsheetId, client.ServiceAccountEmail);
        }

        _values.Remove(sheet.SheetId);
    }

    private SpreadsheetMetadata LoadMetadata()
    {
        try
        {
            var request = client.Service.Spreadsheets.Get(spreadsheetId);
            // Sheet properties only: no cell data.
            request.Fields = "sheets.properties";
            var spreadsheet = request.Execute();

            var sheets = (spreadsheet.Sheets ?? [])
                .Select(s => s.Properties)
                .Where(p => p is not null)
                .Select(p => new SheetInfo(
                    p!.SheetId ?? 0,
                    p.Title ?? string.Empty,
                    p.GridProperties?.RowCount ?? 0,
                    p.GridProperties?.ColumnCount ?? 0))
                .ToList();

            return new SpreadsheetMetadata(sheets);
        }
        catch (GoogleApiException ex)
        {
            throw ex.ToSpreadsheetAccessError(spreadsheetId, client.ServiceAccountEmail);
        }
    }
}
