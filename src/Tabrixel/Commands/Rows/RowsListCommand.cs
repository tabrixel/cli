using Spectre.Console;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;
using Tabrixel.Sheets;

namespace Tabrixel.Commands.Rows;

public class RowsListCommand : CliCommand<RowsListSettings>
{
    protected override ExitCodes ExecuteCommand(CommandContext context, RowsListSettings settings, CancellationToken cancellationToken)
    {
        RowWindow.Validate(settings.Offset, settings.Limit);

        var columnNames = settings.Columns is { } rawColumns ? ParseColumns(rawColumns) : null;

        var spreadsheetId = settings.RequireSpreadsheetId();
        var client = SheetsServiceFactory.Create(settings);
        var spreadsheet = new SpreadsheetContext(client, spreadsheetId);

        var sheet = spreadsheet.ResolveSheet(settings.ResolveSheetName().Value);
        var header = spreadsheet.LoadHeader(sheet);
        var values = spreadsheet.LoadValues(sheet);

        // Paging order is --where → --offset → --limit; total is the matched
        // count before offset and limit, so a caller can tell a truncated or
        // partial page from a complete result. Zero matches are valid data:
        // empty output, exit code 0 — exit code 2 "0 rows affected" belongs to
        // mutations.
        IReadOnlyList<int> matched = settings.Where is { Length: > 0 } rawConditions
            ? WhereMatcher.Match(header, values, rawConditions.Select(WhereCondition.Parse).ToList())
            : Enumerable.Range(1, Math.Max(0, values.Count - 1)).ToList();
        var total = matched.Count;
        matched = RowWindow.Apply(matched, settings.Offset, settings.Limit);

        // --where validates against the full header above, so filtering on a
        // column not selected for output is allowed.
        var outputHeader = columnNames is not null ? header.Project(columnNames) : header;
        var records = RecordReader.Read(outputHeader, values, matched);

        Renderer.Data(new ListPayload(records, total),
            console => RenderTable(console, outputHeader, records, total),
            settings.Format);

        return ExitCodes.Success;
    }

    private sealed record ListPayload(IReadOnlyList<IReadOnlyDictionary<string, string>> Rows, int Total);

    private static IReadOnlyList<string> ParseColumns(string raw)
    {
        var names = raw.Split(',');
        if (names.Any(string.IsNullOrEmpty))
        {
            throw new CliException(ErrorCode.InvalidArguments,
                "--columns must be a comma-separated list of non-empty column names");
        }

        var duplicate = names
            .GroupBy(n => n, StringComparer.Ordinal)
            .FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            throw new CliException(ErrorCode.InvalidArguments,
                $"--columns contains duplicate column '{duplicate.Key}'");
        }

        return names;
    }

    private static void RenderTable(
        IAnsiConsole console,
        SheetHeader header,
        IReadOnlyList<IReadOnlyDictionary<string, string>> records,
        int total)
    {
        var table = new Table();
        foreach (var column in header.Columns)
        {
            table.AddColumn(Markup.Escape(column.Name));
        }

        foreach (var record in records)
        {
            table.AddRow(header.Columns
                .Select(c => Markup.Escape(record[c.Name]))
                .ToArray());
        }

        console.Write(table);
        console.WriteLine($"{records.Count} of {total} record(s)");
    }
}
