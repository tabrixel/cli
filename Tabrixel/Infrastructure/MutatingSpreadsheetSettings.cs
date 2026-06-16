using System.ComponentModel;
using Spectre.Console.Cli;

namespace Tabrixel.Infrastructure;

/// <summary>
/// Base settings for mutating commands that take a positional spreadsheet ID
/// (rows update/delete). Record-payload commands (rows add/upsert) use
/// <see cref="RecordMutationSettings"/> instead, since their positional slot
/// carries the JSON record.
/// </summary>
public class MutatingSpreadsheetSettings : PositionalSpreadsheetSettings
{
    [CommandOption("--dry-run")]
    [Description("Validate and match against the live sheet but write nothing. " +
                 "Output reports what would change; exit codes are the same as a real run.")]
    public bool DryRun { get; set; }
}
