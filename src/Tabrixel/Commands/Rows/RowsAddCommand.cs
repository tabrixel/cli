using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Rows;

public class RowsAddCommand : CliCommand<RowsAddSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, RowsAddSettings settings, CancellationToken cancellationToken)
    {
        // The JSON record is required as a domain error rendered per the output
        // format, not as Spectre parse-error text (consistent with
        // RequireSpreadsheetId), so the positional argument stays optional.
        if (settings.Json is null)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                "JSON record is required: pass it as the first argument, e.g. rows add '{\"Name\":\"John\"}'");
        }

        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        var sheet = spreadsheet.ResolveSheet(settings.ResolveSheetName().Value);
        // A broken header fails the command (HeaderInvalid) before any write.
        var header = spreadsheet.LoadHeader(sheet);
        var row = JsonRecordParser.Parse(settings.Json, header);

        // Real runs get the authoritative row number from the append response.
        // A dry run has no response, so the destination is predicted from the
        // snapshot (one past the last non-empty row) — an estimate that can
        // race with concurrent writers, acceptable for a preview.
        var rowNumber = settings.DryRun
            ? spreadsheet.LoadValues(sheet).Count + 1
            : spreadsheet.AppendRow(sheet, row);

        var verb = settings.DryRun ? "Would add" : "Added";
        Renderer.Data(new AffectedPayload(1, settings.DryRun, rowNumber),
            console => console.MarkupLine(
                $"{verb} 1 record to sheet '{Markup.Escape(sheet.Title)}' (row {rowNumber})."),
            settings.Format);

        return ExitCodes.Success;
    }

    private record AffectedPayload(int Affected, bool DryRun, int Row);
}
