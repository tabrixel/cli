using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Rows;

public class RowsDeleteSettings : MutatingSpreadsheetSettings
{
    [CommandOption("--where <CONDITION>")]
    [Description("Select rows by exact match 'Column=value' (case-sensitive, no type coercion). " +
                 "Repeatable; multiple conditions are combined with AND. 'Column=' matches empty cells.")]
    public string[]? Where { get; set; }

    [CommandOption("--yes")]
    [Description("Required confirmation for deletion. Without it the command does nothing and fails " +
                 "with ConfirmationRequired.")]
    public bool Yes { get; set; }

    [CommandOption("--all")]
    [Description("Delete every matching row when more than one matches.")]
    public bool All { get; set; }

    [CommandOption("--first")]
    [Description("Delete only the first matching row when more than one matches.")]
    public bool First { get; set; }
}
