using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Rows;

public class RowsDeleteCommand : CliCommand<RowsDeleteSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, RowsDeleteSettings settings, CancellationToken cancellationToken)
    {
        // delete requires --yes unless --dry-run (nothing is deleted, so a
        // preview needs no confirmation). Checked first, before creating the
        // client or any API access: without confirmation the sheet is neither
        // read nor modified.
        if (!settings.Yes && !settings.DryRun)
        {
            throw new CliException(ErrorCode.ConfirmationRequired,
                "deletion requires confirmation: pass --yes to delete the matching rows");
        }

        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        var sheet = spreadsheet.ResolveSheet(settings.ResolveSheetName().Value);
        // A broken header fails the command (HeaderInvalid) before any deletion.
        var header = spreadsheet.LoadHeader(sheet);

        var values = spreadsheet.LoadValues(sheet);
        var conditions = (settings.Where ?? []).Select(WhereCondition.Parse).ToList();
        var matched = WhereMatcher.Match(header, values, conditions);
        var selected = MatchSemantics.Select(matched, settings.All, settings.First);

        if (selected.Count == 0)
        {
            // 0 matches is not an error: exit code 2, with a stderr warning in
            // text mode and the regular payload on stdout in JSON mode.
            Renderer.Warning(new MutationPayload<DeletedRowReceipt>(0, 0, settings.DryRun, [], false),
                settings.DryRun
                    ? "0 rows matched the condition; nothing would be deleted."
                    : "0 rows matched the condition; nothing was deleted.",
                settings.Format);
            return ExitCodes.NoMatch;
        }

        // The restore receipt (full records) is taken from the snapshot before
        // the deletion shifts any row numbers.
        var receipts = MutationReceipt.BuildDeleteReceipts(header, values, selected);

        // Index i in values corresponds to 0-based grid row i (the snapshot is
        // aligned with the grid); DeleteRows builds the ranges bottom-up.
        if (!settings.DryRun)
        {
            spreadsheet.DeleteRows(sheet, selected);
        }

        var verb = settings.DryRun ? "Would delete" : "Deleted";
        Renderer.Data(
            new MutationPayload<DeletedRowReceipt>(
                matched.Count, selected.Count, settings.DryRun,
                receipts, selected.Count > MutationReceipt.MaxRows),
            console =>
            {
                console.MarkupLine(
                    $"{verb} {selected.Count} record(s) from sheet '{Markup.Escape(sheet.Title)}'.");
                RenderReceipts(console, header, receipts, selected.Count);
            },
            settings.Format);

        return ExitCodes.Success;
    }

    /// <summary>Per-row summary of the deleted records — the human-readable receipt.</summary>
    private static void RenderReceipts(
        IAnsiConsole console,
        SheetHeader header,
        IReadOnlyList<DeletedRowReceipt> receipts,
        int affected)
    {
        var table = new Table();
        table.AddColumn("Row");
        foreach (var column in header.Columns)
        {
            table.AddColumn(Markup.Escape(column.Name));
        }

        foreach (var receipt in receipts)
        {
            var cells = new List<string> { receipt.Row.ToString() };
            cells.AddRange(header.Columns.Select(c => Markup.Escape(receipt.Data[c.Name])));
            table.AddRow(cells.ToArray());
        }

        console.Write(table);
        if (receipts.Count < affected)
        {
            console.WriteLine($"Showing first {receipts.Count} of {affected} affected row(s).");
        }
    }
}
