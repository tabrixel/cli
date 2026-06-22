using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands;

public class DescribeCommand : CliCommand<SpreadsheetSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, SpreadsheetSettings settings, CancellationToken cancellationToken)
    {
        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        // --sheet given → just that sheet (NotFound lists available ones);
        // omitted → overview of all sheets: describe covers the whole document,
        // so the resolver's single-sheet/SheetAmbiguous branch does not apply.
        // Env/config sheet defaults are deliberately ignored here: a configured
        // default must not silently hide the rest of the document from an overview.
        IReadOnlyList<SheetInfo> sheets = settings.SheetName is not null
            ? [spreadsheet.ResolveSheet(settings.SheetName)]
            : spreadsheet.Metadata.Sheets;

        var descriptions = sheets
            .Select(sheet => SheetDescriber.Describe(sheet.Title, spreadsheet.LoadValues(sheet)))
            .ToList();

        Renderer.Data(BuildPayload(spreadsheetId, descriptions),
            console => RenderTable(console, descriptions),
            settings.Format);

        // A broken header on an individual sheet is not a describe error: the
        // command did its job by showing the document structure.
        return ExitCodes.Success;
    }

    private static Dictionary<string, object?> BuildPayload(
        string spreadsheetId,
        IReadOnlyList<SheetDescription> descriptions)
    {
        var sheets = descriptions.Select(d =>
        {
            var entry = new Dictionary<string, object?> { ["name"] = d.Name };
            if (d.IsValid)
            {
                entry["columns"] = d.Columns;
                entry["records"] = d.Records;
            }
            else
            {
                // Same shape as the CLI error model: code/message/details.
                var error = new Dictionary<string, object?>
                {
                    ["code"] = d.Error!.Code.ToString(),
                    ["message"] = d.Error.Message,
                };
                if (d.Error.Details is not null)
                {
                    error["details"] = d.Error.Details;
                }

                entry["error"] = error;
            }

            return entry;
        }).ToList();

        return new Dictionary<string, object?>
        {
            ["spreadsheet_id"] = spreadsheetId,
            ["sheets"] = sheets,
        };
    }

    private static void RenderTable(IAnsiConsole console, IReadOnlyList<SheetDescription> descriptions)
    {
        var table = new Table();
        table.AddColumn("Sheet");
        table.AddColumn("Records");
        table.AddColumn("Columns");

        foreach (var d in descriptions)
        {
            if (d.IsValid)
            {
                table.AddRow(
                    Markup.Escape(d.Name),
                    d.Records!.Value.ToString(),
                    Markup.Escape(string.Join(", ", d.Columns!)));
            }
            else
            {
                table.AddRow(
                    new Markup(Markup.Escape(d.Name)),
                    new Markup("—"),
                    new Markup($"[red]{Markup.Escape($"{d.Error!.Code}: {d.Error.Message}")}[/]"));
            }
        }

        console.Write(table);
    }
}
