using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Rows;

public class RowsUpsertSettings : MutatingSpreadsheetSettings
{
    [CommandArgument(0, "[JSON]")]
    [Description("Fields to write as a JSON object mapping column names to values, " +
                 "e.g. '{\"Name\":\"John\",\"Age\":30}'. Column names are case-sensitive; " +
                 "unknown fields are rejected. On update only these columns are touched.")]
    public string? Json { get; set; }

    [CommandOption("--where <CONDITION>")]
    [Description("Select rows by exact match 'Column=value' (case-sensitive, no type coercion). " +
                 "Repeatable; multiple conditions are combined with AND; at least one is required. " +
                 "When nothing matches, the condition values become cells of the inserted row.")]
    public string[]? Where { get; set; }

    [CommandOption("--all")]
    [Description("Update every matching row when more than one matches.")]
    public bool All { get; set; }

    [CommandOption("--first")]
    [Description("Update only the first matching row when more than one matches.")]
    public bool First { get; set; }
}
