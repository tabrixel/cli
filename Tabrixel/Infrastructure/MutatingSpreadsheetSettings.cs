using System.ComponentModel;
using Spectre.Console.Cli;

namespace Tabrixel.Infrastructure;

/// <summary>Base settings for commands that modify sheet content (rows add/update/delete).</summary>
public class MutatingSpreadsheetSettings : SpreadsheetSettings
{
    [CommandOption("--dry-run")]
    [Description("Validate and match against the live sheet but write nothing. " +
                 "Output reports what would change; exit codes are the same as a real run.")]
    public bool DryRun { get; set; }
}
