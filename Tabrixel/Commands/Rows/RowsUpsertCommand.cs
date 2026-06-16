using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Rows;

public class RowsUpsertCommand : CliCommand<RowsUpsertSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, RowsUpsertSettings settings, CancellationToken cancellationToken)
    {
        // The JSON record and --where are required as domain errors rendered per
        // the output format, not as Spectre parse-error text.
        if (settings.Json is null)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                "JSON record is required: pass it as the first argument, " +
                "e.g. rows upsert --where 'Email=a@b.c' '{\"Status\":\"Done\"}'");
        }

        // Without --where every data row matches and upsert degenerates into a
        // bulk update; the key condition is mandatory.
        if (settings.Where is not { Length: > 0 })
        {
            throw new CliException(ErrorCode.InvalidArguments,
                "--where is required: pass at least one 'Column=value' key condition, e.g. --where 'Email=a@b.c'");
        }

        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        var sheet = spreadsheet.ResolveSheet(settings.ResolveSheetName().Value);
        // A broken header fails the command (HeaderInvalid) before any write:
        // --where/record column names resolve only against a valid header.
        var header = spreadsheet.LoadHeader(sheet);

        // Both inputs are validated (including the --where/record conflict
        // check) before reading values or writing anything.
        var jsonRow = JsonRecordParser.Parse(settings.Json, header);
        var jsonColumns = JsonColumns(settings.Json, header);
        var conditions = settings.Where.Select(WhereCondition.Parse).ToList();
        RejectConflicts(conditions, jsonRow, jsonColumns, header);

        var values = spreadsheet.LoadValues(sheet);
        var matched = WhereMatcher.Match(header, values, conditions);
        var selected = MatchSemantics.Select(matched, settings.All, settings.First);

        if (selected.Count == 0)
        {
            // Insert path: the new row is the union of the --where key values
            // and the record fields (overlaps are equal by the check above).
            var row = jsonRow.ToArray();
            foreach (var condition in conditions)
            {
                var column = header.FindColumn(condition.ColumnName);
                if (!jsonColumns.Contains(column.Index))
                {
                    row[column.Index] = condition.Value;
                }
            }

            // Real runs get the authoritative row number from the append
            // response; a dry run predicts it from the already-loaded snapshot
            // (one past the last non-empty row) — an estimate that can race
            // with concurrent writers, acceptable for a preview.
            var rowNumber = settings.DryRun
                ? values.Count + 1
                : spreadsheet.AppendRow(sheet, row);

            var insertVerb = settings.DryRun ? "Would insert" : "Inserted";
            Renderer.Data(new UpsertInsertPayload(0, 1, UpsertAction.Inserted, settings.DryRun, rowNumber),
                console => console.MarkupLine(
                    $"{insertVerb} 1 record(s) in sheet '{Markup.Escape(sheet.Title)}' (row {rowNumber})."),
                settings.Format);
            return ExitCodes.Success;
        }

        // Index i in values corresponds to sheet row i + 1 (A1: header is row 1).
        // Only the record-provided columns are rewritten.
        var updates = selected
            .SelectMany(i => jsonColumns.Select(c => new CellUpdate(c, i + 1, jsonRow[c])))
            .ToList();
        if (!settings.DryRun)
        {
            spreadsheet.UpdateCells(sheet, updates);
        }

        // Same receipt shape as rows update: record fields play the role of
        // --set assignments, --where keys identify the record.
        var receipts = MutationReceipt.BuildUpdateReceipts(
            header, values, selected,
            jsonColumns.ToDictionary(c => c, c => jsonRow[c]),
            conditions);

        var updateVerb = settings.DryRun ? "Would update" : "Updated";
        Renderer.Data(
            new UpsertUpdatePayload(
                matched.Count, selected.Count, UpsertAction.Updated, settings.DryRun,
                receipts, selected.Count > MutationReceipt.MaxRows),
            console =>
            {
                console.MarkupLine(
                    $"{updateVerb} {selected.Count} record(s) in sheet '{Markup.Escape(sheet.Title)}'.");
                MutationReceipt.RenderUpdateReceipts(console, receipts, selected.Count);
            },
            settings.Format);

        return ExitCodes.Success;
    }

    /// <summary>
    /// Column indices of the fields actually present in the record. JsonRecordParser
    /// returns a full-width row (absent fields as ""), but an update must touch
    /// only the provided columns, so the field set is re-derived from the
    /// already-validated document.
    /// </summary>
    private static HashSet<int> JsonColumns(string json, SheetHeader header)
    {
        using var document = System.Text.Json.JsonDocument.Parse(json);
        return document.RootElement.EnumerateObject()
            .Select(p => header.FindColumn(p.Name).Index)
            .ToHashSet();
    }

    /// <summary>
    /// A column referenced by both --where and the record must carry the same
    /// cell value (compared after scalar conversion); a different value is
    /// contradictory — there is no single record both inputs describe. Two
    /// --where conditions on one column with different values are rejected for
    /// the same reason: no row (including the inserted one) could ever match,
    /// so every run would append a fresh duplicate.
    /// </summary>
    private static void RejectConflicts(
        IReadOnlyList<WhereCondition> conditions,
        IReadOnlyList<string> jsonRow,
        HashSet<int> jsonColumns,
        SheetHeader header)
    {
        var whereValues = new Dictionary<int, (string Name, string Value)>();
        foreach (var condition in conditions)
        {
            var column = header.FindColumn(condition.ColumnName);
            if (whereValues.TryGetValue(column.Index, out var existing)
                && !string.Equals(existing.Value, condition.Value, StringComparison.Ordinal))
            {
                throw new CliException(ErrorCode.InvalidArguments,
                    $"column '{column.Name}' has conflicting --where values " +
                    $"('{existing.Value}' and '{condition.Value}'); no row could ever match");
            }

            whereValues[column.Index] = (column.Name, condition.Value);
            if (jsonColumns.Contains(column.Index)
                && !string.Equals(jsonRow[column.Index], condition.Value, StringComparison.Ordinal))
            {
                throw new CliException(ErrorCode.InvalidArguments,
                    $"column '{column.Name}' has conflicting values in --where ('{condition.Value}') " +
                    $"and the JSON record ('{jsonRow[column.Index]}')");
            }
        }
    }

    /// <summary>Serialized snake_case by the Renderer enum converter: "updated" / "inserted".</summary>
    private enum UpsertAction
    {
        Updated,
        Inserted,
    }

    // The two outcomes are separate payload types discriminated by Action, so
    // the unused receipt field (Rows vs Row) is omitted instead of null.
    private record UpsertUpdatePayload(
        int Matched, int Affected, UpsertAction Action, bool DryRun,
        IReadOnlyList<UpdatedRowReceipt> Rows, bool Truncated)
    {
        public int Returned => Rows.Count;
    }

    private record UpsertInsertPayload(
        int Matched, int Affected, UpsertAction Action, bool DryRun, int Row);
}
