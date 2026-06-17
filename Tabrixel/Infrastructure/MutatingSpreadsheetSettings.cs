using System.ComponentModel;
using Spectre.Console.Cli;

namespace Tabrixel.Infrastructure;

/// <summary>
/// Single base for all mutating commands (rows add/upsert/update/delete): it adds
/// the shared <c>--dry-run</c> flag on top of the common spreadsheet-ID/sheet
/// resolution in <see cref="SpreadsheetSettings"/>. Record-payload commands
/// (add/upsert) declare their own positional JSON argument; update/delete add
/// their match options.
/// </summary>
public class MutatingSpreadsheetSettings : SpreadsheetSettings
{
    [CommandOption("--dry-run")]
    [Description("Validate and match against the live sheet but write nothing. " +
                 "Output reports what would change; exit codes are the same as a real run.")]
    public bool DryRun { get; set; }
}
