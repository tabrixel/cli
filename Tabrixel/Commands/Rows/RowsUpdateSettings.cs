using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Rows;

public class RowsUpdateSettings : MutatingSpreadsheetSettings
{
    [CommandOption("--where <CONDITION>")]
    [Description("Select rows by exact match 'Column=value' (case-sensitive, no type coercion). " +
                 "Repeatable; multiple conditions are combined with AND. 'Column=' matches empty cells.")]
    public string[]? Where { get; set; }

    [CommandOption("--set <ASSIGNMENT>")]
    [Description("New value for a column as 'Column=value' (case-sensitive). Repeatable; " +
                 "at least one is required. 'Column=' clears the cell. Unknown columns are rejected.")]
    public string[]? Set { get; set; }

    [CommandOption("--all")]
    [Description("Update every matching row when more than one matches.")]
    public bool All { get; set; }

    [CommandOption("--first")]
    [Description("Update only the first matching row when more than one matches.")]
    public bool First { get; set; }
}
