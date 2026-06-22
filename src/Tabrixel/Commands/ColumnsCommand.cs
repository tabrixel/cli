using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands;

public class ColumnsCommand : CliCommand<SpreadsheetSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, SpreadsheetSettings settings, CancellationToken cancellationToken)
    {
        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        var sheet = spreadsheet.ResolveSheet(settings.ResolveSheetName().Value);
        var header = spreadsheet.LoadHeader(sheet);

        Renderer.Data(header.Columns.Select(c => c.Name).ToList(),
            console => RenderTable(console, header),
            settings.Format);

        return ExitCodes.Success;
    }

    private static void RenderTable(IAnsiConsole console, SheetHeader header)
    {
        var table = new Table();
        table.AddColumn("#");
        table.AddColumn("Column");
        foreach (var column in header.Columns)
        {
            table.AddRow(
                Markup.Escape(A1Notation.ColumnLetter(column.Index)),
                Markup.Escape(column.Name));
        }

        console.Write(table);
    }
}
