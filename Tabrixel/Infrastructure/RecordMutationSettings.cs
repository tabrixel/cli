using System.ComponentModel;
using Spectre.Console.Cli;

namespace Tabrixel.Infrastructure;

/// <summary>
/// Base settings for mutating commands whose first positional argument is the
/// JSON record (rows add/upsert), not the spreadsheet ID. The spreadsheet ID
/// resolves from --spreadsheet-id, the env variable, or config. Derives from
/// the non-positional <see cref="SpreadsheetSettings"/>; --dry-run is declared
/// here (and, separately, on the positional MutatingSpreadsheetSettings) because
/// single inheritance cannot share it across both mutating bases.
/// </summary>
public class RecordMutationSettings : SpreadsheetSettings
{
    [CommandOption("--dry-run")]
    [Description("Validate and match against the live sheet but write nothing. " +
                 "Output reports what would change; exit codes are the same as a real run.")]
    public bool DryRun { get; set; }
}
