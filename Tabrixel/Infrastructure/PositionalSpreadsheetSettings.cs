using System.ComponentModel;
using Spectre.Console.Cli;
using Tabrixel.Configuration;

namespace Tabrixel.Infrastructure;

/// <summary>
/// Spreadsheet commands that take the document ID as their first positional
/// argument. The argument is optional at the parser level so that a missing ID
/// produces a domain error rendered per the output format, not Spectre's
/// parse-error text.
/// </summary>
public class PositionalSpreadsheetSettings : SpreadsheetSettings
{
    [CommandArgument(0, "[SPREADSHEET_ID]")]
    [Description("Google Spreadsheets document ID. Takes precedence over --spreadsheet-id.")]
    public string? SpreadsheetIdArgument { get; set; }

    public override string RequireSpreadsheetId() =>
        Require(
            Resolver.Resolve(ConfigKeys.SpreadsheetId, Consts.EnvSpreadsheetId,
                SpreadsheetId, SpreadsheetIdArgument),
            "spreadsheet ID is not set: pass it as an argument, with --spreadsheet-id, " +
            $"via the {Consts.EnvSpreadsheetId} environment variable, " +
            "or with 'config set spreadsheet-id <id>'");
}
