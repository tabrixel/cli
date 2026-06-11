using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Rows;

public class RowsListSettings : SpreadsheetSettings
{
    [CommandOption("--where <CONDITION>")]
    [Description("Filter records by exact match 'Column=value' (case-sensitive, no type coercion). " +
                 "Repeatable; multiple conditions are combined with AND. 'Column=' matches empty cells.")]
    public string[]? Where { get; set; }

    [CommandOption("--limit <N>")]
    [Description("Return at most N first records (default 100). The 'total' field in the " +
                 "response reports how many records matched before the limit.")]
    [DefaultValue(100)]
    public int Limit { get; set; }

    [CommandOption("--columns <NAMES>")]
    [Description("Comma-separated column names to include in the output, in the given order " +
                 "(exact, case-sensitive). Other columns are omitted; --where may still filter on them.")]
    public string? Columns { get; set; }
}
