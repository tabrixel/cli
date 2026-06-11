using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Rows;

public class RowsUpdateCommand : CliCommand<RowsUpdateSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, RowsUpdateSettings settings, CancellationToken cancellationToken)
    {
        // --set is required as a domain error rendered per --output, not as
        // Spectre parse-error text.
        if (settings.Set is not { Length: > 0 })
        {
            throw new CliException(ErrorCode.InvalidArguments,
                "--set is required: pass at least one 'Column=value', e.g. --set 'Status=Done'");
        }

        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        var sheet = spreadsheet.ResolveSheet(settings.ResolveSheetName().Value);
        // A broken header fails the command (HeaderInvalid) before any write:
        // --where/--set column names resolve only against a valid header.
        var header = spreadsheet.LoadHeader(sheet);

        // --set is validated before reading values or writing anything.
        var assignments = ParseAssignments(settings.Set, header);

        var values = spreadsheet.LoadValues(sheet);
        var conditions = (settings.Where ?? []).Select(WhereCondition.Parse).ToList();
        var matched = WhereMatcher.Match(header, values, conditions);
        var selected = MatchSemantics.Select(matched, settings.All, settings.First);

        if (selected.Count == 0)
        {
            // 0 matches is not an error: exit code 2, with a stderr warning in
            // text mode and the regular payload on stdout in JSON mode.
            Renderer.Warning(new MutationPayload<UpdatedRowReceipt>(0, 0, settings.DryRun, [], false),
                settings.DryRun
                    ? "0 rows matched the condition; nothing would be updated."
                    : "0 rows matched the condition; nothing was updated.",
                settings.Format);
            return ExitCodes.NoMatch;
        }

        // Index i in values corresponds to sheet row i + 1 (A1: header is row 1).
        var updates = selected
            .SelectMany(i => assignments.Select(a => new CellUpdate(a.Column.Index, i + 1, a.Value)))
            .ToList();
        if (!settings.DryRun)
        {
            spreadsheet.UpdateCells(sheet, updates);
        }

        // The receipt is built from the pre-write snapshot: Before is what was
        // there, After is the intended result — the agent verifies its own
        // command, not concurrent writers.
        var receipts = MutationReceipt.BuildUpdateReceipts(
            header, values, selected,
            assignments.ToDictionary(a => a.Column.Index, a => a.Value),
            conditions);

        var verb = settings.DryRun ? "Would update" : "Updated";
        Renderer.Data(
            new MutationPayload<UpdatedRowReceipt>(
                matched.Count, selected.Count, settings.DryRun,
                receipts, selected.Count > MutationReceipt.MaxRows),
            console =>
            {
                console.MarkupLine(
                    $"{verb} {selected.Count} record(s) in sheet '{Markup.Escape(sheet.Title)}'.");
                MutationReceipt.RenderUpdateReceipts(console, receipts, selected.Count);
            },
            settings.Format);

        return ExitCodes.Success;
    }

    /// <summary>
    /// Each assignment parses as 'Column=value', the name resolves via
    /// FindColumn (ColumnNotFound with a suggestion); the same column in several
    /// --set options is contradictory → InvalidArguments.
    /// </summary>
    private static IReadOnlyList<(HeaderColumn Column, string Value)> ParseAssignments(
        string[] rawAssignments, SheetHeader header)
    {
        var result = new List<(HeaderColumn Column, string Value)>(rawAssignments.Length);
        var seen = new HashSet<int>();
        foreach (var raw in rawAssignments)
        {
            var assignment = SetAssignment.Parse(raw);
            var column = header.FindColumn(assignment.ColumnName);
            if (!seen.Add(column.Index))
            {
                throw new CliException(ErrorCode.InvalidArguments,
                    $"column '{column.Name}' is set more than once");
            }

            result.Add((column, assignment.Value));
        }

        return result;
    }
}
