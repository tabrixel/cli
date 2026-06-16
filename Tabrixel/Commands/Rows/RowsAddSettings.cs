using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Infrastructure;

namespace Tabrixel.Commands.Rows;

public class RowsAddSettings : RecordMutationSettings
{
    [CommandArgument(0, "[JSON]")]
    [Description("Record to add as a JSON object mapping column names to values, " +
                 "e.g. '{\"Name\":\"John\",\"Age\":30}'. Column names are case-sensitive; " +
                 "unknown fields are rejected. Fields missing from the object are left empty.")]
    public string? Json { get; set; }
}
